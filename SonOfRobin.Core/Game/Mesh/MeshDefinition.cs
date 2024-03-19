using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SonOfRobin
{
    public class MeshDefinition
    {
        public static readonly Dictionary<TextureBank.TextureName, MeshDefinition> meshDefByTextureName = new();
        private static readonly List<MeshDefinition> meshDefBySearchPriority = new();

        public readonly TextureBank.TextureName textureName;
        public readonly TextureBank.TextureName mapTextureName;
        public readonly Texture2D texture;
        public readonly Texture2D mapTexture;

        public readonly MeshGenerator.RawMapDataSearchForTexture search;
        public readonly Level.LevelType[] levelTypes;
        public readonly int drawPriority;
        public readonly BlendState blendState;
        public EffInstance effInstance;

        public static readonly Tweener tweener = new();
        public float tweenEffectPower;
        public Vector2 tweenBaseTextureOffset;

        public MeshDefinition(Level.LevelType[] levelTypes, TextureBank.TextureName textureName, TextureBank.TextureName mapTextureName, MeshGenerator.RawMapDataSearchForTexture search, int drawPriority = 0, BlendState blendState = default)
        {
            this.levelTypes = levelTypes;

            this.textureName = textureName;
            this.mapTextureName = mapTextureName;
            this.texture = TextureBank.GetTexture(textureName);
            this.mapTexture = TextureBank.GetTexture(mapTextureName);

            this.blendState = blendState == default ? BlendState.AlphaBlend : blendState;
            this.search = search;
            this.drawPriority = drawPriority;

            this.tweenEffectPower = 0f;
            this.tweenBaseTextureOffset = Vector2.Zero;

            meshDefByTextureName[textureName] = this;
        }

        public static void UpdateAllDefs()
        {
            tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public static MeshDefinition[] GetMeshDefBySearchPriority(Level.LevelType levelType)
        {
            return meshDefBySearchPriority.Where(meshDef => meshDef.levelTypes.Contains(levelType)).OrderBy(meshDef => meshDef.search.searchPriority).ToArray();
        }

        public static void CreateMeshDefinitions()
        {
            // needed to fill small holes, that will occur between other meshes
            MeshDefinition groundBase = new MeshDefinition(
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
                textureName: TextureBank.TextureName.RepeatingGroundBase,
                mapTextureName: TextureBank.TextureName.RepeatingMapGroundBase,
                drawPriority: -1,
                search: new(
                searchPriority: 0,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 255),
                    })
                );

            MeshDefinition waterDeep = new MeshDefinition(
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
                textureName: TextureBank.TextureName.RepeatingWaterSuperShallow,
                mapTextureName: TextureBank.TextureName.RepeatingMapWaterSuperShallow,
                blendState: new BlendState
                {
                    AlphaBlendFunction = BlendFunction.ReverseSubtract,
                    AlphaSourceBlend = Blend.One,
                    AlphaDestinationBlend = Blend.One,

                    ColorBlendFunction = BlendFunction.Add,
                    ColorSourceBlend = Blend.One,
                    ColorDestinationBlend = Blend.One,
                },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
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
                levelTypes: new Level.LevelType[] { Level.LevelType.Island, Level.LevelType.Cave },
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

            lava.tweenEffectPower = 0f;
            lava.effInstance = new MeshLavaInstance(meshDef: lava);

            tweener.TweenTo(target: lava, expression: meshDef => meshDef.tweenEffectPower, toValue: 1f, duration: 15, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);

            MeshDefinition swamp = new MeshDefinition(
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
                textureName: TextureBank.TextureName.RepeatingSwamp,
                mapTextureName: TextureBank.TextureName.RepeatingMapSwamp,
                search: new(
                searchPriority: 8,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: true)})
                );
            swamp.tweenEffectPower = 0.5f;
            swamp.effInstance = new MeshSwampInstance(meshDef: swamp);

            tweener.TweenTo(target: swamp, expression: meshDef => meshDef.tweenEffectPower, toValue: 1f, duration: 60 * 4, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.SineInOut);

            MeshDefinition ruins = new MeshDefinition(
                levelTypes: new Level.LevelType[] { Level.LevelType.Island },
                textureName: TextureBank.TextureName.RepeatingPebblesColor, // RepeatingRuins
                mapTextureName: TextureBank.TextureName.RepeatingMapRuins,
                search: new(
                searchPriority: 11,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: true)})
                );

            ruins.effInstance = new MeshNormalMapInstance(meshDef: ruins, normalMapTexture: TextureBank.GetTexture(textureName: TextureBank.TextureName.RepeatingPebblesNormal), flippedNormalYAxis: true, lightPowerMultiplier: 0.2f);

            MeshDefinition caveWall = new MeshDefinition(
                levelTypes: new Level.LevelType[] { Level.LevelType.Cave },
                textureName: TextureBank.TextureName.RepeatingCaveWall,
                mapTextureName: TextureBank.TextureName.RepeatingMapCaveWall,
                search: new(
                searchPriority: 1,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 0, maxVal: 0),
                    })
                );

            MeshDefinition caveFloor = new MeshDefinition(
                levelTypes: new Level.LevelType[] { Level.LevelType.Cave },
                textureName: TextureBank.TextureName.RepeatingCaveFloor,
                mapTextureName: TextureBank.TextureName.RepeatingMapCaveFloor,
                drawPriority: -1,
                search: new(
                searchPriority: 0,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 1, maxVal: 255), // should cover whole floor, to avoid small holes
                    })
                );

            foreach (MeshDefinition meshDef in meshDefByTextureName.Values)
            {
                // every meshDef should have its effect defined
                if (meshDef.effInstance == null) meshDef.effInstance = new MeshBasicInstance(meshDef: meshDef);
            }

            meshDefBySearchPriority.AddRange(meshDefByTextureName.Values.OrderBy(meshDef => meshDef.search.searchPriority));
        }
    }
}