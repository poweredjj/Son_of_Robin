using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

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

            string textureName = "repeating textures/mountain_low";

            var pointList = new List<Point>();

            foreach (NamedLocations.Location location in grid.namedLocations.locationList)
            {
                foreach (Cell cell in location.cells)
                {
                    pointList.Add(new Point(cell.cellNoX, cell.cellNoY));
                }
            }

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

            Mesh mesh = ConvertShapesToMesh(offset: new Vector2(xMin * grid.cellWidth, yMin * grid.cellHeight), scaleX: grid.cellWidth, scaleY: grid.cellHeight, textureName: textureName, groupedShapes: groupedShapes);

            meshList.Add(mesh);

            // test code end

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
    }
}