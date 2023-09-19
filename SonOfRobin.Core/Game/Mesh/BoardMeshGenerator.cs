using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class BoardMeshGenerator
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

        public static Mesh[] GenerateMeshes(Grid grid)
        {
            var pixelBagsForPatterns = SplitRawPixelsBySearchCategories(grid: grid, meshDefs: MeshDefinition.meshDefBySearchPriority.ToArray());
            var meshBag = new ConcurrentBag<Mesh>();

            //foreach (RawMapDataSearch search in searches) // for profiling in debugger
            Parallel.ForEach(MeshDefinition.meshDefBySearchPriority, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, meshDef =>
            {
                var pixelCoordsByRegion = Helpers.SlicePointBagIntoConnectedRegions(width: grid.dividedWidth, height: grid.dividedHeight, pointsBag: pixelBagsForPatterns[meshDef.textureName]);

                foreach (List<Point> pointList in pixelCoordsByRegion)
                {
                    if (pointList.Count == 0) continue;

                    int xMin = int.MaxValue;
                    int xMax = int.MinValue;
                    int yMin = int.MaxValue;
                    int yMax = int.MinValue;

                    foreach (Point point in pointList)
                    {
                        xMin = Math.Min(xMin, point.X);
                        xMax = Math.Max(xMax, point.X);
                        yMin = Math.Min(yMin, point.Y);
                        yMax = Math.Max(yMax, point.Y);
                    }

                    int width = xMax - xMin + 1;
                    int height = yMax - yMin + 1;

                    BitArrayWrapper bitArrayWrapper = new(width, height);
                    foreach (Point point in CollectionsMarshal.AsSpan(pointList))
                    {
                        bitArrayWrapper.SetVal(x: point.X - xMin, y: point.Y - yMin, value: true);
                    }
                    pointList.Clear(); // no longer needed, clearing memory

                    // splitting very large bitmaps into chunks, because triangulation has size limit
                    // it is a little glitchy, but necessary at this point
                    foreach (BitArrayWrapperChunk chunk in bitArrayWrapper.SplitIntoChunks(chunkWidth: 2000, chunkHeight: 2000, xOverlap: 2, yOverlap: 2)) // 2500, 2500
                    {
                        var groupedShapes = BitmapToShapesConverter.GenerateShapes(chunk);

                        Mesh mesh = ConvertShapesToMesh(
                            offset: new Vector2((xMin + chunk.xOffset) * grid.resDivider, (yMin + chunk.yOffset) * grid.resDivider),
                            scaleX: grid.resDivider, scaleY: grid.resDivider,
                            textureName: meshDef.textureName,
                            groupedShapes: groupedShapes);

                        List<Mesh> splitMeshes = mesh.SplitIntoChunks(maxChunkSize: 800);
                        foreach (Mesh splitMesh in splitMeshes)
                        {
                            if (mesh.indices.Length >= 3) meshBag.Add(splitMesh);
                        }
                    }
                }
                //} // for profiling in debugger
            });

            var meshByID = new Dictionary<string, Mesh>();
            foreach (Mesh mesh in meshBag)
            {
                meshByID[mesh.meshID] = mesh; // filtering out duplicated meshes
            }

            var meshArray = meshByID.Values.ToArray();

            SaveToTemplate(meshesFilePath: GetMeshesFilePath(grid), meshArray: meshArray);
            return meshArray;
        }

        public static Dictionary<TextureBank.TextureName, ConcurrentBag<Point>> SplitRawPixelsBySearchCategories(Grid grid, MeshDefinition[] meshDefs)
        {
            var pixelBagsForPatterns = new Dictionary<TextureBank.TextureName, ConcurrentBag<Point>>();
            foreach (MeshDefinition meshDef in meshDefs)
            {
                pixelBagsForPatterns[meshDef.textureName] = new ConcurrentBag<Point>();
            }

            var rawPixelsBag = new ConcurrentBag<Point>();

            Parallel.For(0, grid.dividedHeight, rawY =>
            {
                for (int rawX = 0; rawX < grid.dividedWidth; rawX++)
                {
                    for (int i = 0; i < meshDefs.Length; i++)
                    {
                        MeshDefinition meshDef = meshDefs[i];
                        if (meshDef.search.PixelMeetsCriteria(grid: grid, rawX: rawX, rawY: rawY))
                        {
                            pixelBagsForPatterns[meshDef.textureName].Add(new Point(rawX, rawY));
                            if (!meshDef.search.otherSearchesAllowed) break;
                        }
                    }
                }
            });

            return pixelBagsForPatterns;
        }

        public static Mesh[] LoadFromTemplate(string meshesFilePath)
        {
            var loadedData = FileReaderWriter.Load(path: meshesFilePath);
            if (loadedData == null) return null;

            var loadedDict = (Dictionary<string, Object>)loadedData;

            float loadedVersion = (float)(double)loadedDict["version"];
            if (loadedVersion != Mesh.currentVersion) return null;

            var meshListSerialized = (List<Object>)loadedDict["meshList"];

            var meshBag = new ConcurrentBag<Mesh>();
            Parallel.ForEach(meshListSerialized, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, meshData =>
            {
                Mesh mesh = new Mesh(meshData);
                if (mesh.indices.Length >= 3) meshBag.Add(mesh);
            });

            return meshBag.ToArray();
        }

        public static void SaveToTemplate(string meshesFilePath, Mesh[] meshArray)
        {
            var meshBagSerialized = new List<Object> { };
            foreach (Mesh mesh in meshArray)
            {
                meshBagSerialized.Add(mesh.Serialize());
            }

            Dictionary<string, Object> meshData = new()
            {
                { "version", Mesh.currentVersion },
                { "meshList", meshBagSerialized },
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
                    if (!TerrainMeetsCriteria(grid: grid, rawX: rawX, rawY: rawY, terrainName: searchEntryTerrain.name, minVal: searchEntryTerrain.minVal, maxVal: searchEntryTerrain.maxVal)) return false;
                }

                foreach (SearchEntryExtProps searchEntryExtProps in this.searchEntriesExtProps)
                {
                    if (grid.ExtBoardProps.GetValueRaw(name: searchEntryExtProps.name, rawX: rawX, rawY: rawY) != searchEntryExtProps.value) return false;
                }

                return true;
            }

            private static bool TerrainMeetsCriteria(Grid grid, int rawX, int rawY, Terrain.Name terrainName, byte minVal, byte maxVal)
            {
                byte value = grid.terrainByName[terrainName].GetMapDataRaw(rawX, rawY);
                return value >= minVal && value <= maxVal;
            }
        }
    }
}