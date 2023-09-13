using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class BoardMeshGenerator
    {
        private const string meshesFileName = "meshes.json";

        public static List<Mesh> GenerateMeshes(Grid grid)
        {
            string meshesFilePath = Path.Combine(grid.gridTemplate.templatePath, meshesFileName);

            List<Mesh> loadedMeshes = LoadFromTemplate(meshesFilePath);
            if (loadedMeshes != null) return loadedMeshes;

            var meshList = new List<Mesh>();

            // TODO replace test code with search similar to NamedLocations
            // test code

            List<RawMapDataSearch> searches = new();

            searches.Add(new(textureName: "repeating textures/mountain_low",
                 searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 0, maxVal: 75),
                            new SearchEntryTerrain(name: Terrain.Name.Biome, minVal: 0, maxVal: Terrain.biomeMin),
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax, maxVal: Terrain.rocksLevelMin),
                    },
                    searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.OuterBeach, value: false) }
            ));

            foreach (RawMapDataSearch search in searches)
            {
                var pixelCoordsByRegion = Helpers.SlicePointBagIntoConnectedRegions(width: grid.dividedWidth, height: grid.dividedHeight, pointsBag: search.FindAllRawPixelsThatMeetCriteria(grid));

                foreach (List<Point> pointList in pixelCoordsByRegion)
                {

                    if (pointList.Count == 0) return meshList;

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

                    var boolArray = new bool[width, height];
                    foreach (Point point in pointList)
                    {
                        boolArray[point.X - xMin, point.Y - yMin] = true;
                    }

                    var groupedShapes = BitmapToShapesConverter.GenerateShapes(boolArray);

                    Mesh mesh = ConvertShapesToMesh(offset: new Vector2(xMin * grid.resDivider, yMin * grid.resDivider), scaleX: grid.resDivider, scaleY: grid.resDivider, textureName: search.textureName, groupedShapes: groupedShapes);

                    meshList.Add(mesh);
                }
            }


            SaveToTemplate(meshesFilePath: meshesFilePath, meshList: meshList);
            return meshList;
        }

        public static List<Mesh> LoadFromTemplate(string meshesFilePath)
        {
            var loadedData = FileReaderWriter.Load(path: meshesFilePath);
            if (loadedData == null) return null;

            var meshList = new List<Mesh>();
            foreach (object meshData in (List<Object>)loadedData)
            {
                meshList.Add(new Mesh(meshData));
            }

            return meshList;
        }

        public static void SaveToTemplate(string meshesFilePath, List<Mesh> meshList)
        {
            var meshData = new List<Object> { };
            foreach (Mesh mesh in meshList)
            {
                meshData.Add(mesh.Serialize());
            }

            FileReaderWriter.Save(path: meshesFilePath, savedObj: meshData, compress: true);
        }

        public static Mesh ConvertShapesToMesh(Vector2 offset, float scaleX, float scaleY, Dictionary<BitmapToShapesConverter.Shape, List<BitmapToShapesConverter.Shape>> groupedShapes, string textureName)
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
                foreach (var edge in contour.edges)
                {
                    doublesList.Add(edge.start.X);
                    doublesList.Add(edge.start.Y);
                    vertPositionsForShape.Add(edge.start);
                    currentIndex++;
                }

                foreach (var hole in holes)
                {
                    holeIndices.Add(currentIndex);

                    foreach (var edge in hole.edges)
                    {
                        doublesList.Add(edge.start.X);
                        doublesList.Add(edge.start.Y);
                        vertPositionsForShape.Add(edge.start);
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

        public readonly struct RawMapDataSearch
        {
            public readonly string textureName;
            public readonly List<SearchEntryTerrain> searchEntriesTerrain;
            public readonly List<SearchEntryExtProps> searchEntriesExtProps;

            public RawMapDataSearch(string textureName, List<SearchEntryTerrain> searchEntriesTerrain = null, List<SearchEntryExtProps> searchEntriesExtProps = null)
            {
                this.textureName = textureName;

                if (searchEntriesTerrain == null) searchEntriesTerrain = new List<SearchEntryTerrain>();
                if (searchEntriesExtProps == null) searchEntriesExtProps = new List<SearchEntryExtProps>();

                if (searchEntriesTerrain.Count == 0 && searchEntriesExtProps.Count == 0) throw new ArgumentException("Invalid search criteria.");

                this.searchEntriesTerrain = searchEntriesTerrain;
                this.searchEntriesExtProps = searchEntriesExtProps;
            }

            public ConcurrentBag<Point> FindAllRawPixelsThatMeetCriteria(Grid grid)
            {
                var rawPixelsBag = new ConcurrentBag<Point>();

                RawMapDataSearch search = this; // needed for parallel access

                Parallel.For(0, grid.dividedHeight, rawY =>
                {
                    for (int rawX = 0; rawX < grid.dividedWidth; rawX++)
                    {
                        if (search.PixelMeetsCriteria(grid: grid, rawX: rawX, rawY: rawY)) rawPixelsBag.Add(new Point(rawX, rawY));
                    }
                });

                return rawPixelsBag;
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