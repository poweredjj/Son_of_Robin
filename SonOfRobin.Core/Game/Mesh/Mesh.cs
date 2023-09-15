using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public readonly struct Mesh
    {
        public const float currentVersion = 1.005f;

        public readonly string textureName;
        public readonly Texture2D texture;
        public readonly VertexPositionTexture[] vertices;
        public readonly short[] indices;
        public readonly int triangleCount;
        public readonly Rectangle boundsRect;
        public readonly int drawPriority;

        public Mesh(string textureName, List<VertexPositionTexture> vertList, List<short> indicesList, int drawPriority, Grid grid)
        {
            var vertices = vertList.ToArray();

            this.textureName = textureName;
            this.texture = TextureBank.GetTexture(this.textureName);
            this.vertices = vertices;
            this.indices = indicesList.ToArray();
            this.triangleCount = this.indices.Length / 3;
            Rectangle boundsRect = GetBoundsRect(vertices);
            this.boundsRect = boundsRect;
            this.drawPriority = drawPriority;

            var overlappingCells = grid.GetCellsInsideRect(rectangle: boundsRect, addPadding: false);
            var assignedCellsIndexes = new int[overlappingCells.Count];

            for (int i = 0; i < overlappingCells.Count; i++)
            {
                assignedCellsIndexes[i] = overlappingCells[i].cellIndex;
            }
        }

        public Mesh(object meshData)
        {
            var meshDict = (Dictionary<string, Object>)meshData;

            this.textureName = (string)meshDict["textureName"];
            this.texture = TextureBank.GetTexture(this.textureName);

            this.indices = (short[])meshDict["indices"];
            this.triangleCount = this.indices.Length / 3;

            this.boundsRect = (Rectangle)meshDict["boundsRect"];
            this.drawPriority = (int)(Int64)meshDict["drawPriority"];

            var vertXPos = (List<float>)meshDict["vertXPos"];
            var vertYPos = (List<float>)meshDict["vertYPos"];
            var vertTexCoordX = (List<float>)meshDict["vertTexCoordX"];
            var vertTexCoordY = (List<float>)meshDict["vertTexCoordY"];

            var vertList = new List<VertexPositionTexture>();

            for (int i = 0; i < vertXPos.Count; i++)
            {
                VertexPositionTexture vertex = new();
                vertex.Position = new Vector3(vertXPos[i], vertYPos[i], 0);
                vertex.TextureCoordinate = new Vector2(vertTexCoordX[i], vertTexCoordY[i]);

                vertList.Add(vertex);
            }

            this.vertices = vertList.ToArray();
        }

        public Object Serialize()
        {
            var vertXPos = new List<float>();
            var vertYPos = new List<float>();
            var vertTexCoordX = new List<float>();
            var vertTexCoordY = new List<float>();

            foreach (VertexPositionTexture vertex in this.vertices)
            {
                vertXPos.Add(vertex.Position.X);
                vertYPos.Add(vertex.Position.Y);
                vertTexCoordX.Add(vertex.TextureCoordinate.X);
                vertTexCoordY.Add(vertex.TextureCoordinate.Y);
            }

            Dictionary<string, Object> meshData = new()
            {
                { "textureName", this.textureName },
                { "indices", this.indices },
                { "boundsRect", this.boundsRect },
                { "drawPriority", this.drawPriority },
                { "vertXPos", vertXPos },
                { "vertYPos", vertYPos },
                { "vertTexCoordX", vertTexCoordX },
                { "vertTexCoordY", vertTexCoordY },
            };

            return meshData;
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
                PrimitiveType.TriangleList, this.vertices, 0, this.vertices.Length, indices, 0, this.triangleCount);
        }
    }

    public readonly struct MeshGrid
    {
        public readonly int totalWidth;
        public readonly int totalHeight;
        public readonly int blockWidth;
        public readonly int blockHeight;
        public readonly int numBlocksX;
        public readonly int numBlocksY;

        public readonly List<Mesh>[,] meshListGrid;

        public MeshGrid(int totalWidth, int totalHeight, int blockWidth, int blockHeight, List<Mesh> meshList)
        {
            if (totalWidth <= 0 || totalWidth <= 0) throw new ArgumentException($"Invalid total size {totalWidth}x{totalWidth}.");
            if (blockWidth <= 0 || blockHeight <= 0) throw new ArgumentException($"Invalid block size {blockWidth}x{blockHeight}.");

            blockWidth = Math.Min(blockWidth, totalWidth);
            blockHeight = Math.Min(blockHeight, totalHeight);

            this.totalWidth = totalWidth;
            this.totalHeight = totalHeight;
            this.blockWidth = blockWidth;
            this.blockHeight = blockHeight;

            int numBlocksX = (int)Math.Ceiling((double)totalWidth / (double)blockWidth);
            int numBlocksY = (int)Math.Ceiling((double)totalHeight / (double)blockHeight);

            this.numBlocksX = numBlocksX;
            this.numBlocksY = numBlocksY;

            var meshListGrid = new List<Mesh>[numBlocksX, numBlocksY];

            Rectangle blockRect = new(x: 0, y: 0, width: blockWidth, height: blockHeight);

            for (int blockNoX = 0; blockNoX < numBlocksX; blockNoX++)
            {
                for (int blockNoY = 0; blockNoY < numBlocksY; blockNoY++)
                {
                    blockRect.X = blockNoX * blockWidth;
                    blockRect.Y = blockNoY * blockHeight;

                    var blockMeshList = meshListGrid[blockNoX, blockNoY];

                    foreach (Mesh mesh in meshList)
                    {
                        if (mesh.boundsRect.Intersects(blockRect)) blockMeshList.Add(mesh);
                    }
                }
            }

            this.meshListGrid = meshListGrid;
        }

        public List<Mesh> GetMeshesForRect(Rectangle rect)
        {
            List<Mesh> meshesInRect = new();

            // Calculate the block range that overlaps with the given rectangle.
            int startBlockX = Math.Max(0, rect.Left / blockWidth);
            int endBlockX = Math.Min(meshListGrid.GetLength(0) - 1, rect.Right / blockWidth);
            int startBlockY = Math.Max(0, rect.Top / blockHeight);
            int endBlockY = Math.Min(meshListGrid.GetLength(1) - 1, rect.Bottom / blockHeight);

            for (int blockNoX = startBlockX; blockNoX <= endBlockX; blockNoX++)
            {
                for (int blockNoY = startBlockY; blockNoY <= endBlockY; blockNoY++)
                {
                    // Check if the block's meshes intersect with the given rectangle.
                    foreach (Mesh mesh in meshListGrid[blockNoX, blockNoY])
                    {
                        meshesInRect.Add(mesh);
                    }
                }
            }

            return meshesInRect;
        }
    }
}