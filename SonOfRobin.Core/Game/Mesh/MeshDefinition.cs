﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public readonly Texture2D texture;
        public readonly Texture2D mapTexture;

        public readonly MeshGenerator.RawMapDataSearchForTexture search;
        public readonly int drawPriority;
        public readonly BlendState blendState;

        public readonly Tweener tweener;
        public float textureOffsetX;
        public float textureOffsetY;
        public float textureDeformationOffsetX;
        public float textureDeformationOffsetY;
        public bool TweenerActive { get; private set; }
        public Vector2 TextureOffset { get; private set; }

        public MeshDefinition(TextureBank.TextureName textureName, TextureBank.TextureName mapTextureName, BlendState blendState, MeshGenerator.RawMapDataSearchForTexture search, int drawPriority = 0)
        {
            this.textureName = textureName;
            this.mapTextureName = mapTextureName;
            this.texture = TextureBank.GetTexture(textureName);
            this.mapTexture = TextureBank.GetTexture(mapTextureName);

            this.blendState = blendState;
            this.search = search;
            this.drawPriority = drawPriority;

            this.tweener = new Tweener();
            this.textureOffsetX = 0f;
            this.textureOffsetY = 0f;
            this.textureDeformationOffsetX = 0f;
            this.textureDeformationOffsetY = 0f;

            this.TweenerActive = false;
            this.TextureOffset = Vector2.Zero;

            meshDefByTextureName[textureName] = this;
        }

        public static void UpdateAllDefs()
        {
            foreach (MeshDefinition meshDef in meshDefBySearchPriority)
            {
                meshDef.Update();
            }
        }

        private void Update()
        {
            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            this.TextureOffset = new Vector2(this.textureOffsetX, this.textureOffsetY);

            this.TweenerActive = this.TextureOffset != Vector2.Zero ||
                this.textureDeformationOffsetX != 0 ||
                this.textureDeformationOffsetY != 0;
        }

        public static void CreateMeshDefinitions()
        {
            // needed to fill small holes, that will occur between other meshes
            MeshDefinition groundBase = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGroundBase,
                mapTextureName: TextureBank.TextureName.RepeatingMapGroundBase,
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                textureName: TextureBank.TextureName.RepeatingBeachBright,
                mapTextureName: TextureBank.TextureName.RepeatingMapBeachBright,
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
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
                blendState: BlendState.AlphaBlend,
                search: new(
                searchPriority: 17,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.lavaMin, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            lava.tweener.TweenTo(target: lava, expression: meshDef => meshDef.textureOffsetX, toValue: 0.1f, duration: 20, delay: 5)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);

            lava.tweener.TweenTo(target: lava, expression: meshDef => meshDef.textureOffsetY, toValue: 0.1f, duration: 20, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);

            lava.tweener.TweenTo(target: lava, expression: meshDef => meshDef.textureDeformationOffsetX, toValue: 0.2f, duration: 20, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.SineInOut);

            lava.tweener.TweenTo(target: lava, expression: meshDef => meshDef.textureDeformationOffsetY, toValue: 0.07f, duration: 5.0f, delay: 0)
                .RepeatForever(repeatDelay: 2.3f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);

            MeshDefinition swamp = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingSwamp,
                mapTextureName: TextureBank.TextureName.RepeatingMapSwamp,
                blendState: BlendState.AlphaBlend,
                search: new(
                searchPriority: 8,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: true)})
                );

            swamp.tweener.TweenTo(target: swamp, expression: meshDef => meshDef.textureDeformationOffsetX, toValue: 0.11f, duration: 12, delay: 2)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.SineInOut);

            swamp.tweener.TweenTo(target: swamp, expression: meshDef => meshDef.textureDeformationOffsetY, toValue: 0.11f, duration: 20, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.SineInOut);

            MeshDefinition ruins = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingRuins,
                mapTextureName: TextureBank.TextureName.RepeatingMapRuins,
                blendState: BlendState.AlphaBlend,
                search: new(
                searchPriority: 11,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: true)})
                );

            meshDefBySearchPriority.AddRange(meshDefByTextureName.Values.OrderBy(meshDef => meshDef.search.searchPriority));
        }
    }
}