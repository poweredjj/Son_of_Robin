﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class MeshGenerator
    {
        private const string meshesFileName = "meshes.json";

        private static string GetMeshesFilePath(Grid grid)
        {
            return Path.Combine(grid.gridTemplate.templatePath, meshesFileName);
        }

        public static Mesh[] LoadMeshes(Grid grid)
        {
            return LoadFromTemplate(GetMeshesFilePath(grid));
        }

        public static MeshGrid CreateMeshGrid(Mesh[] meshArray, Grid grid)
        {
            return new(totalWidth: grid.width, totalHeight: grid.height, blockWidth: 2000, blockHeight: 2000, inputMeshArray: meshArray);
        }

        public readonly struct ChunkDataForMeshGeneration
        {
            public readonly MeshDefinition meshDef;
            public readonly BitArrayWrapperChunk chunk;

            public ChunkDataForMeshGeneration(MeshDefinition meshDef, BitArrayWrapperChunk chunk)
            {
                this.meshDef = meshDef;
                this.chunk = chunk;
            }
        }

        public static Mesh[] GenerateMeshes(Grid grid, bool saveTemplate)
        {
            MeshDefinition[] meshDefs = MeshDefinition.GetMeshDefBySearchPriority(grid.level.levelType);
            var pixelBagsForTextureNames = GetRawPixelsForMeshDefSearches(grid: grid, meshDefs: meshDefs);

            ConcurrentBag<ChunkDataForMeshGeneration> chunkDataForMeshGenBag = new();

            Parallel.ForEach(meshDefs, SonOfRobinGame.defaultParallelOptions, meshDef =>
            {
                ConcurrentBag<Point> pixelBag = pixelBagsForTextureNames[meshDef.textureName];

                if (!pixelBag.IsEmpty)
                {
                    BitArrayWrapper bitArrayWrapper = new(grid.dividedWidth, grid.dividedHeight);

                    Span<Point> pixelArrayAsSpan = pixelBag.ToArray().AsSpan();

                    for (int i = 0; i < pixelArrayAsSpan.Length; i++)
                    {
                        bitArrayWrapper.SetVal(x: pixelArrayAsSpan[i].X, y: pixelArrayAsSpan[i].Y, value: true);
                    }

                    // Splitting very large bitmaps into chunks (to optimize drawing and because triangulation has size limits).
                    // Keeping chunk size random, to make shared chunk borders less common.

                    foreach (BitArrayWrapperChunk chunk in bitArrayWrapper.SplitIntoChunks(
                        chunkWidth: Math.Max(grid.world.random.Next(800, 1200) / grid.resDivider, 40),
                        chunkHeight: Math.Max(grid.world.random.Next(800, 1200) / grid.resDivider, 40)))
                    {
                        if (chunk.HasAnyPixelSet) chunkDataForMeshGenBag.Add(new ChunkDataForMeshGeneration(meshDef: meshDef, chunk: chunk));
                    }
                }
            });

            // Iterating (with parallel) over chunk data is more efficient, than iterating over mesh defs (as above).

            var meshBag = new ConcurrentBag<Mesh>();

            Parallel.ForEach(chunkDataForMeshGenBag, SonOfRobinGame.defaultParallelOptions, chunkDataForMeshGen =>
            {
                Mesh mesh = ConvertShapesToMesh(
                    offset: new Vector2(chunkDataForMeshGen.chunk.xOffset * grid.resDivider, chunkDataForMeshGen.chunk.yOffset * grid.resDivider),
                    scaleX: grid.resDivider, scaleY: grid.resDivider,
                    textureName: chunkDataForMeshGen.meshDef.textureName,
                    groupedShapes: BitmapToShapesConverter.GenerateShapes(chunkDataForMeshGen.chunk));

                if (mesh.indices.Length >= 3) meshBag.Add(mesh);
            });

            Mesh[] meshArray = meshBag.ToArray();

            if (saveTemplate) SaveToTemplate(meshesFilePath: GetMeshesFilePath(grid), meshArray: meshArray);
            return meshArray;
        }

        public static Dictionary<TextureBank.TextureName, ConcurrentBag<Point>> GetRawPixelsForMeshDefSearches(Grid grid, MeshDefinition[] meshDefs)
        {
            var pixelBagsForTextureNames = new Dictionary<TextureBank.TextureName, ConcurrentBag<Point>>();
            foreach (MeshDefinition meshDef in meshDefs)
            {
                pixelBagsForTextureNames[meshDef.textureName] = new ConcurrentBag<Point>();
            }

            var rawPixelsBag = new ConcurrentBag<Point>();

            Parallel.For(0, grid.dividedHeight, rawY =>
            {
                Span<MeshDefinition> meshDefsAsSpan = meshDefs.AsSpan();

                for (int rawX = 0; rawX < grid.dividedWidth; rawX++)
                {
                    for (int i = 0; i < meshDefsAsSpan.Length; i++)
                    {
                        MeshDefinition meshDef = meshDefsAsSpan[i];
                        if (meshDef.search.PixelMeetsCriteria(grid: grid, rawX: rawX, rawY: rawY))
                        {
                            pixelBagsForTextureNames[meshDef.textureName].Add(new Point(rawX, rawY));
                            if (!meshDef.search.otherSearchesAllowed) break;
                        }
                    }
                }
            });

            return pixelBagsForTextureNames;
        }

        public static Mesh[] LoadFromTemplate(string meshesFilePath)
        {
            var loadedData = FileReaderWriter.Load(path: meshesFilePath);
            if (loadedData == null) return null;

            var loadedDict = (Dictionary<string, Object>)loadedData;

            float loadedVersion = (float)(double)loadedDict["version"];
            if (loadedVersion != Mesh.currentVersion) return null;

            var meshListSerialized = (Object[])loadedDict["meshArray"];

            var meshBag = new ConcurrentBag<Mesh>();
            Parallel.ForEach(meshListSerialized, SonOfRobinGame.defaultParallelOptions, meshData =>
            {
                meshBag.Add(new Mesh(meshData));
            });

            return meshBag.ToArray();
        }

        public static void SaveToTemplate(string meshesFilePath, Mesh[] meshArray)
        {
            var meshBagSerialized = new ConcurrentBag<Object> { };

            Parallel.ForEach(meshArray, SonOfRobinGame.defaultParallelOptions, mesh =>
            {
                meshBagSerialized.Add(mesh.Serialize());
            });

            Dictionary<string, Object> meshData = new()
            {
                { "version", Mesh.currentVersion },
                { "meshArray", meshBagSerialized.ToArray() },
            };
            FileReaderWriter.Save(path: meshesFilePath, savedObj: meshData, compress: true);
        }

        public static Mesh ConvertShapesToMesh(Vector2 offset, float scaleX, float scaleY, Dictionary<BitmapToShapesConverter.Shape, List<BitmapToShapesConverter.Shape>> groupedShapes, TextureBank.TextureName textureName)
        {
            Texture2D texture = TextureBank.GetTexture(textureName);
            Vector2 textureSize = new(texture.Width, texture.Height);

            var vertList = new List<VertexPositionTexture>();
            var indicesList = new List<short>();

            foreach (var kvp in groupedShapes)
            {
                var doublesList = new List<double>(); // flat array of vertex coordinates like x0, y0, x1, y1, x2, y2...
                var holeIndices = new List<int>(); // first vertex of each hole (points at doublesList, not accounting for double coords kept there)
                var contour = kvp.Key;
                var holes = kvp.Value;

                var vertPositionsForShape = new List<Vector2>();
                var shapeVertList = new List<VertexPositionTexture>();

                int currentIndex = 0;
                foreach (Vector2 pos in contour.pointList)
                {
                    doublesList.Add(pos.X);
                    doublesList.Add(pos.Y);
                    vertPositionsForShape.Add(pos);
                    currentIndex++;
                }

                foreach (var hole in holes)
                {
                    holeIndices.Add(currentIndex);

                    foreach (Vector2 pos in hole.pointList)
                    {
                        doublesList.Add(pos.X);
                        doublesList.Add(pos.Y);
                        vertPositionsForShape.Add(pos);
                        currentIndex++;
                    }
                }

                List<int> indicesForShape = Earcut.Tessellate(data: doublesList.ToArray(), holeIndices: holeIndices.ToArray());

                Vector3 basePos = new(offset.X, offset.Y, 0);

                foreach (Vector2 position in vertPositionsForShape)
                {
                    VertexPositionTexture vertex = new();
                    vertex.Position = basePos + new Vector3(position.X * scaleX, position.Y * scaleY, 0);
                    vertex.TextureCoordinate = new Vector2(vertex.Position.X / textureSize.X, vertex.Position.Y / textureSize.Y);
                    shapeVertList.Add(vertex);
                }

                foreach (int indice in indicesForShape)
                {
                    indicesList.Add((short)(indice + vertList.Count));
                }

                vertList.AddRange(shapeVertList);
            }

            return new Mesh(textureName: textureName, vertList: vertList, indicesList: indicesList);
        }

        public readonly struct RawMapDataSearchForTexture
        {
            public readonly List<SearchEntryTerrain> searchEntriesTerrain;
            public readonly List<SearchEntryExtProps> searchEntriesExtProps;
            public readonly int searchPriority;
            public readonly bool otherSearchesAllowed;

            public RawMapDataSearchForTexture(List<SearchEntryTerrain> searchEntriesTerrain = null, List<SearchEntryExtProps> searchEntriesExtProps = null, bool otherSearchesAllowed = true, int searchPriority = 0)
            {
                if (searchEntriesTerrain == null) searchEntriesTerrain = new List<SearchEntryTerrain>();
                if (searchEntriesExtProps == null) searchEntriesExtProps = new List<SearchEntryExtProps>();

                if (searchEntriesTerrain.Count == 0 && searchEntriesExtProps.Count == 0) throw new ArgumentException("Invalid search criteria.");

                this.searchEntriesTerrain = searchEntriesTerrain;
                this.searchEntriesExtProps = searchEntriesExtProps;

                this.searchPriority = searchPriority;
                this.otherSearchesAllowed = otherSearchesAllowed;
            }

            public bool PixelMeetsCriteria(Grid grid, int rawX, int rawY)
            {
                foreach (SearchEntryTerrain searchEntryTerrain in this.searchEntriesTerrain)
                {
                    byte terrainVal = grid.terrainByName[searchEntryTerrain.name].GetMapDataRaw(rawX, rawY);
                    if (!(terrainVal >= searchEntryTerrain.minVal && terrainVal <= searchEntryTerrain.maxVal)) return false;
                }

                foreach (SearchEntryExtProps searchEntryExtProps in this.searchEntriesExtProps)
                {
                    if (grid.ExtBoardProps.GetValueRaw(name: searchEntryExtProps.name, rawX: rawX, rawY: rawY) != searchEntryExtProps.value) return false;
                }

                return true;
            }
        }
    }
}