using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public readonly struct Mesh
    {
        public readonly Texture2D texture;
        public readonly VertexPositionTexture[] vertArray;
        public readonly short[] indices;
        public readonly int triangleCount;
        public readonly Rectangle boundsRect;

        public Mesh(Texture2D texture, List<VertexPositionTexture> vertList, List<short> indicesList)
        {
            var vertArray = vertList.ToArray();

            this.texture = texture;
            this.vertArray = vertArray;
            this.indices = indicesList.ToArray();
            this.triangleCount = this.indices.Length / 3;
            this.boundsRect = GetBoundsRect(vertArray);
        }

        private static Rectangle GetBoundsRect(VertexPositionTexture[] vertArray)
        {
            int xMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMin = int.MaxValue;
            int yMax = int.MinValue;

            foreach (VertexPositionTexture vertex in vertArray)
            {
                xMin = Math.Min(xMin, (int)vertex.Position.X);
                xMax = Math.Max(xMax, (int)vertex.Position.X);
                yMin = Math.Min(yMin, (int)vertex.Position.Y);
                yMax = Math.Max(yMax, (int)vertex.Position.Y);
            }

            return new Rectangle(x: xMin, y: yMin, width: xMax - xMin, height: yMax - yMin);
        }

        public void Draw()
        {
            SonOfRobinGame.GfxDev.DrawUserIndexedPrimitives<VertexPositionTexture>(
                PrimitiveType.TriangleList, this.vertArray, 0, this.vertArray.Length, indices, 0, this.triangleCount);
        }

        public static Mesh ConvertShapesToMesh(Vector2 offset, Dictionary<MarchingSquaresMeshGenerator.Shape, List<MarchingSquaresMeshGenerator.Shape>> groupedShapes, Texture2D texture)
        {
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
                    vertex.Position = basePos + new Vector3(position.X * 20, position.Y * 20, 0);
                    vertex.TextureCoordinate = new Vector2(vertex.Position.X / textureSize.X, vertex.Position.Y / textureSize.Y);
                    shapeVertList.Add(vertex);
                }

                foreach (int indice in indicesForShape)
                {
                    indicesList.Add((short)(indice + vertList.Count));
                }

                vertList.AddRange(shapeVertList);
            }

            return new Mesh(texture: texture, vertList: vertList, indicesList: indicesList);
        }
    }
}