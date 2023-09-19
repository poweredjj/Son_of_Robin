﻿using Microsoft.Xna.Framework;
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
            List<RawMapDataSearchForTexture> searchesUnsorted = new()
            {
                new(
                // needed to fill small holes, that will occur between other meshes
                textureName: TextureBank.TextureName.RepeatingGroundBase, drawPriority: -1, otherSearchesAllowed: true,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)}),

                new(
                textureName: TextureBank.TextureName.RepeatingWaterDeep, // transparent textures should not overlap
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 0, maxVal: (byte)(Terrain.waterLevelMax / 3)),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)}),

                new(
                textureName: TextureBank.TextureName.RepeatingWaterMedium, // transparent textures should not overlap
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: (byte)(Terrain.waterLevelMax / 3) + 1, maxVal: (byte)(Terrain.waterLevelMax / 3 * 2)),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                // water_shallow is not listed, because it is just transparent (no mesh needed)

                new(
                textureName: TextureBank.TextureName.RepeatingWaterSuperShallow, otherSearchesAllowed: false,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax - 3, maxVal: Terrain.waterLevelMax),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingBeachBright,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 95),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingBeachDark,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 96, maxVal: 105),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingSand,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 0, maxVal: 75),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingGroundBad,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 76, maxVal: 115),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingGroundGood,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 116, maxVal: 120),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingGrassBad,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 121, maxVal: 160),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingGrassGood,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 161, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingMountainLow,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.rocksLevelMin, maxVal: 178),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingMountainMedium,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 179, maxVal: 194),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingMountainHigh,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 195, maxVal: Terrain.volcanoEdgeMin - 1),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingVolcanoEdge,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.volcanoEdgeMin, maxVal: Terrain.lavaMin - 1),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingLava,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.lavaMin, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingSwamp,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: true),
                }),

                new(
                textureName: TextureBank.TextureName.RepeatingRuins,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: true),
                }),
            };

            // searches should be ordered from most to least common, to speed up search
            var searchOrder = new List<TextureBank.TextureName>
            {
                TextureBank.TextureName.RepeatingWaterDeep,
                TextureBank.TextureName.RepeatingGrassBad,
                TextureBank.TextureName.RepeatingMountainLow,
                TextureBank.TextureName.RepeatingGroundBad,
                TextureBank.TextureName.RepeatingBeachBright,
                TextureBank.TextureName.RepeatingBeachDark,
                TextureBank.TextureName.RepeatingWaterMedium,
                TextureBank.TextureName.RepeatingSwamp,
                TextureBank.TextureName.RepeatingGrassGood,
                TextureBank.TextureName.RepeatingMountainMedium,
                TextureBank.TextureName.RepeatingRuins,
                TextureBank.TextureName.RepeatingWaterSuperShallow,
                TextureBank.TextureName.RepeatingGroundGood,
                TextureBank.TextureName.RepeatingMountainHigh,
                TextureBank.TextureName.RepeatingSand,
                TextureBank.TextureName.RepeatingVolcanoEdge,
                TextureBank.TextureName.RepeatingLava,

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

            //foreach (RawMapDataSearch search in searches) // for profiling in debugger
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

                    // splitting very large bitmaps into chunks, because triangulation has size limit
                    // it is a little glitchy, but necessary at this point
                    foreach (BitArrayWrapperChunk chunk in bitArrayWrapper.SplitIntoChunks(chunkWidth: 2000, chunkHeight: 2000, xOverlap: 2, yOverlap: 2)) // 2500, 2500
                    {
                        var groupedShapes = BitmapToShapesConverter.GenerateShapes(chunk);

                        Mesh mesh = ConvertShapesToMesh(
                            offset: new Vector2((xMin + chunk.xOffset) * grid.resDivider, (yMin + chunk.yOffset) * grid.resDivider),
                            scaleX: grid.resDivider, scaleY: grid.resDivider,
                            textureName: search.textureName,
                            drawPriority: search.drawPriority,
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

        public static Dictionary<TextureBank.TextureName, ConcurrentBag<Point>> SplitRawPixelsBySearchCategories(Grid grid, RawMapDataSearchForTexture[] searches)
        {
            var pixelBagsForPatterns = new Dictionary<TextureBank.TextureName, ConcurrentBag<Point>>();
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

        public static Mesh ConvertShapesToMesh(Vector2 offset, float scaleX, float scaleY, Dictionary<BitmapToShapesConverter.Shape, List<BitmapToShapesConverter.Shape>> groupedShapes, TextureBank.TextureName textureName, int drawPriority)
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

            return new Mesh(textureName: textureName, vertList: vertList, indicesList: indicesList, drawPriority: drawPriority);
        }

        public readonly struct RawMapDataSearchForTexture
        {
            public readonly TextureBank.TextureName textureName;
            public readonly List<SearchEntryTerrain> searchEntriesTerrain;
            public readonly List<SearchEntryExtProps> searchEntriesExtProps;
            public readonly bool otherSearchesAllowed;
            public readonly int drawPriority;

            public RawMapDataSearchForTexture(TextureBank.TextureName textureName, List<SearchEntryTerrain> searchEntriesTerrain = null, List<SearchEntryExtProps> searchEntriesExtProps = null, bool otherSearchesAllowed = true, int drawPriority = 1)
            {
                this.textureName = textureName;

                if (searchEntriesTerrain == null) searchEntriesTerrain = new List<SearchEntryTerrain>();
                if (searchEntriesExtProps == null) searchEntriesExtProps = new List<SearchEntryExtProps>();

                if (searchEntriesTerrain.Count == 0 && searchEntriesExtProps.Count == 0) throw new ArgumentException("Invalid search criteria.");

                this.searchEntriesTerrain = searchEntriesTerrain;
                this.searchEntriesExtProps = searchEntriesExtProps;

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