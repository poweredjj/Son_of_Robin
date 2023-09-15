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

        public static Mesh[] GenerateMeshes(Grid grid)
        {
            string meshesFilePath = Path.Combine(grid.gridTemplate.templatePath, meshesFileName);
            Mesh[] loadedMeshes = LoadFromTemplate(meshesFilePath);
            if (loadedMeshes != null) return loadedMeshes;

            List<RawMapDataSearchForTexture> searchesUnsorted = new()
            {
                new(
                // needed to fill small holes, that will occur between other meshes
                textureName: RepeatingPattern.Name.ground_base, drawPriority: -1, otherSearchesAllowed: true,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)}),

                new(
                textureName: RepeatingPattern.Name.water_deep, canOverlap: false, // transparent textures should not overlap
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 0, maxVal: (byte)(Terrain.waterLevelMax / 3)),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)}),

                new(
                textureName: RepeatingPattern.Name.water_medium, canOverlap: false, // transparent textures should not overlap
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: (byte)(Terrain.waterLevelMax / 3) + 1, maxVal: (byte)(Terrain.waterLevelMax / 3 * 2)),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                // water_shallow is not listed, because it is just transparent (no mesh needed)

                new(
                textureName: RepeatingPattern.Name.water_supershallow, canOverlap: false, otherSearchesAllowed: true,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax - 3, maxVal: Terrain.waterLevelMax),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.beach_bright,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 95),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.beach_dark,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 96, maxVal: 105),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.sand,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 0, maxVal: 75),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.ground_bad,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 76, maxVal: 115),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.ground_good,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 116, maxVal: 120),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.grass_bad,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 121, maxVal: 160),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.grass_good,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 161, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.mountain_low,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.rocksLevelMin, maxVal: 178),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.mountain_medium,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 179, maxVal: 194),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.mountain_high,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 195, maxVal: Terrain.volcanoEdgeMin - 1),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.volcano_edge,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.volcanoEdgeMin, maxVal: Terrain.lavaMin - 1),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.lava,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.lavaMin, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: RepeatingPattern.Name.swamp,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: true),
                }),

                new(
                textureName: RepeatingPattern.Name.ruins,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: true),
                }),
            };

            // searches should be ordered from most to least common, to speed up search
            var searchOrder = new List<RepeatingPattern.Name>
            {
                RepeatingPattern.Name.water_deep,
                RepeatingPattern.Name.grass_bad,
                RepeatingPattern.Name.mountain_low,
                RepeatingPattern.Name.ground_bad,
                RepeatingPattern.Name.beach_bright,
                RepeatingPattern.Name.beach_dark,
                RepeatingPattern.Name.water_medium,
                RepeatingPattern.Name.swamp,
                RepeatingPattern.Name.grass_good,
                RepeatingPattern.Name.mountain_medium,
                RepeatingPattern.Name.ruins,
                RepeatingPattern.Name.water_supershallow,
                RepeatingPattern.Name.ground_good,
                RepeatingPattern.Name.mountain_high,
                RepeatingPattern.Name.sand,
                RepeatingPattern.Name.volcano_edge,
                RepeatingPattern.Name.lava,

                // names not specified here will be added at the end
            };

            var searches = searchesUnsorted.OrderBy(search => search.otherSearchesAllowed).ThenBy(search => searchOrder.IndexOf(search.textureName)).ToList();

            // Add any missing items at the end
            foreach (RawMapDataSearchForTexture search in searchesUnsorted)
            {
                if (!searches.Contains(search)) searches.Add(search);
            }

            var pixelBagsForPatterns = SplitRawPixelsBySearchCategories(grid: grid, searches: searches.ToArray());
            var meshBag = new ConcurrentBag<Mesh>();

            //foreach (RawMapDataSearch search in searches)
            Parallel.ForEach(searches, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, search =>
            {
                var pixelCoordsByRegion = Helpers.SlicePointBagIntoConnectedRegions(width: grid.dividedWidth, height: grid.dividedHeight, pointsBag: pixelBagsForPatterns[search.textureName]);

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

                    int overlap = search.canOverlap ? 2 : 0;

                    int chunkSize = Math.Max(1000 / grid.resDivider, 15); // 1000

                    foreach (var chunk in bitArrayWrapper.SplitIntoChunks(chunkWidth: chunkSize, chunkHeight: chunkSize, xOverlap: overlap, yOverlap: overlap))
                    {
                        var groupedShapes = BitmapToShapesConverter.GenerateShapes(chunk);

                        Mesh mesh = ConvertShapesToMesh(
                            offset: new Vector2((chunk.xOffset + xMin) * grid.resDivider, (chunk.yOffset + yMin) * grid.resDivider),
                            scaleX: grid.resDivider, scaleY: grid.resDivider,
                            textureName: $"repeating textures/{search.textureName}",
                            drawPriority: search.drawPriority,
                            grid: grid,
                            groupedShapes: groupedShapes);

                        meshBag.Add(mesh);
                    }
                }
                //}
            });

            var meshArray = meshBag.ToArray();

            SaveToTemplate(meshesFilePath: meshesFilePath, meshArray: meshArray);
            return meshArray;
        }

        public static Dictionary<RepeatingPattern.Name, ConcurrentBag<Point>> SplitRawPixelsBySearchCategories(Grid grid, RawMapDataSearchForTexture[] searches)
        {
            var pixelBagsForPatterns = new Dictionary<RepeatingPattern.Name, ConcurrentBag<Point>>();
            foreach (RawMapDataSearchForTexture search in searches)
            {
                pixelBagsForPatterns[search.textureName] = new ConcurrentBag<Point>();
            }

            var rawPixelsBag = new ConcurrentBag<Point>();

            Parallel.For(0, grid.dividedHeight, rawY =>
            {
                for (int rawX = 0; rawX < grid.dividedWidth; rawX++)
                {
                    for (int i = 0; i < searches.Length; i++)
                    {
                        RawMapDataSearchForTexture search = searches[i];
                        if (search.PixelMeetsCriteria(grid: grid, rawX: rawX, rawY: rawY))
                        {
                            pixelBagsForPatterns[search.textureName].Add(new Point(rawX, rawY));
                            if (!search.otherSearchesAllowed) break;
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
                meshBag.Add(new Mesh(meshData));
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

        public static Mesh ConvertShapesToMesh(Vector2 offset, float scaleX, float scaleY, Dictionary<BitmapToShapesConverter.Shape, List<BitmapToShapesConverter.Shape>> groupedShapes, string textureName, int drawPriority, Grid grid)
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

            return new Mesh(textureName: textureName, vertList: vertList, indicesList: indicesList, drawPriority: drawPriority, grid: grid);
        }

        public readonly struct RawMapDataSearchForTexture
        {
            public readonly RepeatingPattern.Name textureName;
            public readonly List<SearchEntryTerrain> searchEntriesTerrain;
            public readonly List<SearchEntryExtProps> searchEntriesExtProps;
            public readonly bool canOverlap;
            public readonly bool otherSearchesAllowed;
            public readonly int drawPriority;

            public RawMapDataSearchForTexture(RepeatingPattern.Name textureName, List<SearchEntryTerrain> searchEntriesTerrain = null, List<SearchEntryExtProps> searchEntriesExtProps = null, bool canOverlap = true, bool otherSearchesAllowed = true, int drawPriority = 1)
            {
                this.textureName = textureName;

                if (searchEntriesTerrain == null) searchEntriesTerrain = new List<SearchEntryTerrain>();
                if (searchEntriesExtProps == null) searchEntriesExtProps = new List<SearchEntryExtProps>();

                if (searchEntriesTerrain.Count == 0 && searchEntriesExtProps.Count == 0) throw new ArgumentException("Invalid search criteria.");

                this.searchEntriesTerrain = searchEntriesTerrain;
                this.searchEntriesExtProps = searchEntriesExtProps;

                this.canOverlap = canOverlap;
                this.otherSearchesAllowed = otherSearchesAllowed;
                this.drawPriority = drawPriority;
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