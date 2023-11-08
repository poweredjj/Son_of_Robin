using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
            public readonly int posX;
            public readonly int posY;

            public ChunkDataForMeshGeneration(MeshDefinition meshDef, BitArrayWrapperChunk chunk, int posX, int posY)
            {
                this.meshDef = meshDef;
                this.chunk = chunk;
                this.posX = posX;
                this.posY = posY;
            }
        }

        public static Mesh[] GenerateMeshes(Grid grid, bool saveTemplate)
        {
            DateTime stageStartTime = DateTime.Now;

            MeshDefinition[] meshDefs = MeshDefinition.GetMeshDefBySearchPriority(grid.level.levelType);
            var pixelBagsForTextureNames = GetRawPixelsForMeshDefSearches(grid: grid, meshDefs: meshDefs);

            ConcurrentBag<ChunkDataForMeshGeneration> chunkDataForMeshGenBag = new();

            int totalRawPixelCount = grid.dividedWidth * grid.dividedHeight;

            MessageLog.Add(debugMessage: true, text: $"mesh gen details - creating pixel bags: {DateTime.Now - stageStartTime:hh\\:mm\\:ss\\.fff}");
            stageStartTime = DateTime.Now;

            Parallel.ForEach(meshDefs, SonOfRobinGame.defaultParallelOptions, meshDef =>
            {
                ConcurrentBag<Point> pixelBag = pixelBagsForTextureNames[meshDef.textureName];

                List<Point[]> pixelCoordsArrayByRegion = new();

                if (pixelBag.Count < totalRawPixelCount * 0.03f)
                {
                    var pixelCoordsListByRegion = Helpers.SlicePointBagIntoConnectedRegions(width: grid.dividedWidth, height: grid.dividedHeight, pointsBag: pixelBag);
                    foreach (List<Point> pointList in pixelCoordsListByRegion)
                    {
                        pixelCoordsArrayByRegion.Add(pointList.ToArray());
                    }
                }
                else
                {
                    pixelCoordsArrayByRegion.Add(pixelBag.ToArray());
                }

                pixelBag.Clear(); // clearing memory

                foreach (Point[] pixelArray in pixelCoordsArrayByRegion)
                {
                    if (pixelArray.Length > 0)
                    {
                        Span<Point> pixelArrayAsSpan = pixelArray.AsSpan();
                        int xMin, xMax, yMin, yMax;

                        bool calculateWholeGrid = pixelArray.Length > totalRawPixelCount * 0.05f;

                        if (calculateWholeGrid)
                        {
                            xMin = 0;
                            yMin = 0;
                            xMax = grid.dividedWidth - 1;
                            yMax = grid.dividedHeight - 1;
                        }
                        else
                        {
                            xMin = int.MaxValue;
                            xMax = int.MinValue;
                            yMin = int.MaxValue;
                            yMax = int.MinValue;

                            for (int i = 0; i < pixelArrayAsSpan.Length; i++)
                            {
                                Point point = pixelArrayAsSpan[i];
                                if (point.X < xMin) xMin = point.X;
                                if (point.X > xMax) xMax = point.X;
                                if (point.Y < yMin) yMin = point.Y;
                                if (point.Y > yMax) yMax = point.Y;
                            }
                        }

                        int width = xMax - xMin + 1;
                        int height = yMax - yMin + 1;

                        BitArrayWrapper bitArrayWrapper = new(width, height);

                        for (int i = 0; i < pixelArrayAsSpan.Length; i++)
                        {
                            bitArrayWrapper.SetVal(x: pixelArrayAsSpan[i].X - xMin, y: pixelArrayAsSpan[i].Y - yMin, value: true);
                        }

                        // Splitting very large bitmaps into chunks (to optimize drawing and because triangulation has size limits).
                        // Keeping chunk size random, to make shared chunk borders less common.

                        foreach (BitArrayWrapperChunk chunk in bitArrayWrapper.SplitIntoChunks(
                            chunkWidth: Math.Max(grid.world.random.Next(1200, 2200) / grid.resDivider, 80),
                            chunkHeight: Math.Max(grid.world.random.Next(1200, 1600) / grid.resDivider, 80)))
                        {
                            if (chunk.HasAnyPixelSet) chunkDataForMeshGenBag.Add(new ChunkDataForMeshGeneration(meshDef: meshDef, chunk: chunk, posX: xMin + chunk.xOffset, posY: yMin + chunk.yOffset));
                        }
                    }
                }
            });

            MessageLog.Add(debugMessage: true, text: $"mesh gen details - creating chunk data: {DateTime.Now - stageStartTime:hh\\:mm\\:ss\\.fff}");
            stageStartTime = DateTime.Now;

            // Iterating (with parallel) over chunk data is more efficient, than iterating over mesh defs (as above).

            var meshBag = new ConcurrentBag<Mesh>();

            Parallel.ForEach(chunkDataForMeshGenBag, SonOfRobinGame.defaultParallelOptions, chunkDataForMeshGen =>
            {
                Mesh mesh = ConvertShapesToMesh(
                    offset: new Vector2(chunkDataForMeshGen.posX * grid.resDivider, chunkDataForMeshGen.posY * grid.resDivider),
                    scaleX: grid.resDivider, scaleY: grid.resDivider,
                    textureName: chunkDataForMeshGen.meshDef.textureName,
                    groupedShapes: BitmapToShapesConverter.GenerateShapes(chunkDataForMeshGen.chunk));

                if (mesh.indices.Length >= 3) meshBag.Add(mesh);
            });

            MessageLog.Add(debugMessage: true, text: $"mesh gen details - mesh creation: {DateTime.Now - stageStartTime:hh\\:mm\\:ss\\.fff}");
            stageStartTime = DateTime.Now;

            Mesh[] meshArray = meshBag.ToArray();

            if (saveTemplate) SaveToTemplate(meshesFilePath: GetMeshesFilePath(grid), meshArray: meshArray);
            MessageLog.Add(debugMessage: true, text: $"mesh gen details - mesh saving: {DateTime.Now - stageStartTime:hh\\:mm\\:ss\\.fff}");

            return meshArray;
        }

        public static Dictionary<TextureBank.TextureName, ConcurrentBag<Point>> GetRawPixelsForMeshDefSearches(Grid grid, MeshDefinition[] meshDefs)
        {
            var pixelBagsForTextureNames = new Dictionary<TextureBank.TextureName, ConcurrentBag<Point>>();
            foreach (MeshDefinition meshDef in meshDefs)
            {
                pixelBagsForTextureNames[meshDef.textureName] = new ConcurrentBag<Point>();
            }

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
                var contour = kvp.Key;
                var holes = kvp.Value;

                int elementCount = contour.pointList.Count;
                foreach (var hole in holes) elementCount += hole.pointList.Count;

                double[] doublesArray = new double[elementCount * 2]; // flat array of vertex coordinates like x0, y0, x1, y1, x2, y2...
                int[] holeIndicesArray = new int[holes.Count]; // first vertex of each hole (points at doublesList, not accounting for double coords kept there)
                Vector2[] vertPositionsForShapeArray = new Vector2[elementCount];

                Span<double> doublesArrayAsSpan = doublesArray.AsSpan();
                Span<Vector2> vertPositionsForShapeArrayAsSpan = vertPositionsForShapeArray.AsSpan();

                var shapeVertList = new List<VertexPositionTexture>();

                int currentIndex = 0;

                var contourPointListAsSpan = CollectionsMarshal.AsSpan(contour.pointList);
                for (var i = 0; i < contourPointListAsSpan.Length; i++)
                {
                    Vector2 pos = contourPointListAsSpan[i];

                    doublesArrayAsSpan[currentIndex * 2] = pos.X;
                    doublesArrayAsSpan[(currentIndex * 2) + 1] = pos.Y;
                    vertPositionsForShapeArrayAsSpan[currentIndex] = pos;
                    currentIndex++;
                }

                for (int i = 0; i < holes.Count; i++)
                {
                    BitmapToShapesConverter.Shape hole = holes[i];
                    holeIndicesArray[i] = currentIndex;

                    var holePointListAsSpan = CollectionsMarshal.AsSpan(hole.pointList);
                    for (var j = 0; j < holePointListAsSpan.Length; j++)
                    {
                        Vector2 pos = holePointListAsSpan[j];

                        doublesArrayAsSpan[currentIndex * 2] = pos.X;
                        doublesArrayAsSpan[(currentIndex * 2) + 1] = pos.Y;
                        vertPositionsForShapeArrayAsSpan[currentIndex] = pos;
                        currentIndex++;
                    }
                }

                List<int> indicesForShape = Earcut.Tessellate(data: doublesArray, holeIndices: holeIndicesArray);
                var indicesForShapeAsSpan = CollectionsMarshal.AsSpan(indicesForShape);

                Vector3 basePos = new(offset.X, offset.Y, 0);

                for (int i = 0; i < vertPositionsForShapeArrayAsSpan.Length; i++)
                {
                    Vector2 position = vertPositionsForShapeArrayAsSpan[i];
                    VertexPositionTexture vertex = new() { Position = basePos + new Vector3(position.X * scaleX, position.Y * scaleY, 0) };
                    vertex.TextureCoordinate = new Vector2(vertex.Position.X / textureSize.X, vertex.Position.Y / textureSize.Y);
                    shapeVertList.Add(vertex);
                }

                for (int i = 0; i < indicesForShapeAsSpan.Length; i++)
                {
                    indicesList.Add((short)(indicesForShapeAsSpan[i] + vertList.Count));
                }

                vertList.AddRange(shapeVertList);
            }

            return new Mesh(textureName: textureName, vertArray: vertList.ToArray(), indicesArray: indicesList.ToArray());
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
                    byte terrainVal = grid.terrainByName[searchEntryTerrain.name].GetMapDataRawNoBoundsCheck(rawX, rawY);
                    if (terrainVal < searchEntryTerrain.minVal || terrainVal > searchEntryTerrain.maxVal) return false;
                }

                foreach (SearchEntryExtProps searchEntryExtProps in this.searchEntriesExtProps)
                {
                    if (grid.ExtBoardProps.GetValueRaw(name: searchEntryExtProps.name, rawX: rawX, rawY: rawY, boundsCheck: false) != searchEntryExtProps.value) return false;
                }

                return true;
            }
        }
    }
}