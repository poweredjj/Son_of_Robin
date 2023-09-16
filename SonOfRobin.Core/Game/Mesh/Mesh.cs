using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public readonly struct Mesh
    {
        public const float currentVersion = 1.009f;

        public readonly string meshID;
        public readonly string textureName;
        public readonly Texture2D texture;
        public readonly VertexPositionTexture[] vertices;
        public readonly short[] indices;
        public readonly int triangleCount;
        public readonly Rectangle boundsRect;
        public readonly int drawPriority;

        public Mesh(string textureName, List<VertexPositionTexture> vertList, List<short> indicesList, int drawPriority)
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
            this.meshID = GetID(boundsRect: boundsRect, textureName: textureName);
        }

        private static string GetID(Rectangle boundsRect, string textureName)
        {
            // needed to filter out duplicates
            return $"{boundsRect.Left},{boundsRect.Top}_{boundsRect.Width}x{boundsRect.Height}_{textureName}";
        }

        public Mesh(object meshData)
        {
            var meshDict = (Dictionary<string, Object>)meshData;

            this.textureName = (string)meshDict["textureName"];
            this.texture = TextureBank.GetTexture(this.textureName);

            this.indices = (short[])meshDict["indices"];
            this.triangleCount = this.indices.Length / 3;

            this.boundsRect = (Rectangle)meshDict["boundsRect"];
            this.meshID = GetID(boundsRect: boundsRect, textureName: textureName);

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

        public List<Mesh> SplitIntoChunks(int maxChunkSize)
        {
            // return new List<Mesh> { this }; // for testing

            if (maxChunkSize <= 0) throw new ArgumentException($"Invalid chunk size {maxChunkSize}.");

            int numChunksX = (int)Math.Ceiling((double)this.boundsRect.Width / (double)maxChunkSize);
            int numChunksY = (int)Math.Ceiling((double)this.boundsRect.Height / (double)maxChunkSize);
            if (numChunksX == 1 && numChunksY == 1) return new List<Mesh> { this };

            var newIndicesArray = new List<short>[numChunksX, numChunksY];
            var newVerticesArray = new List<VertexPositionTexture>[numChunksX, numChunksY];

            for (int blockNoX = 0; blockNoX < numChunksX; blockNoX++)
            {
                for (int blockNoY = 0; blockNoY < numChunksY; blockNoY++)
                {
                    newIndicesArray[blockNoX, blockNoY] = new List<short>();
                    newVerticesArray[blockNoX, blockNoY] = new List<VertexPositionTexture>();
                }
            }

            for (int i = 0; i < this.indices.Length; i += 3)
            {
                try
                {
                    VertexPositionTexture vertex1 = this.vertices[indices[i]];
                    VertexPositionTexture vertex2 = this.vertices[indices[i + 1]];
                    VertexPositionTexture vertex3 = this.vertices[indices[i + 2]];

                    float xMin = Math.Min(Math.Min(vertex1.Position.X, vertex2.Position.X), vertex3.Position.X);
                    float yMin = Math.Min(Math.Min(vertex1.Position.Y, vertex2.Position.Y), vertex3.Position.Y);

                    int xBlockNo = (int)((xMin - this.boundsRect.X) / maxChunkSize);
                    int yBlockNo = (int)((yMin - this.boundsRect.Y) / maxChunkSize);

                    short currentVecticesCount = (short)newVerticesArray[xBlockNo, yBlockNo].Count;

                    newVerticesArray[xBlockNo, yBlockNo].Add(vertex1);
                    newVerticesArray[xBlockNo, yBlockNo].Add(vertex2);
                    newVerticesArray[xBlockNo, yBlockNo].Add(vertex3);

                    newIndicesArray[xBlockNo, yBlockNo].Add(currentVecticesCount);
                    newIndicesArray[xBlockNo, yBlockNo].Add((short)(currentVecticesCount + 1));
                    newIndicesArray[xBlockNo, yBlockNo].Add((short)(currentVecticesCount + 2));
                }
                catch (IndexOutOfRangeException)
                {
                    MessageLog.AddMessage(debugMessage: true, message: $"Mesh {this.textureName} - index {i} out of bounds", color: Color.Orange);
                    continue;
                }
            }

            var chunkMeshes = new List<Mesh>();

            for (int blockNoX = 0; blockNoX < numChunksX; blockNoX++)
            {
                for (int blockNoY = 0; blockNoY < numChunksY; blockNoY++)
                {
                    if (newVerticesArray[blockNoX, blockNoY].Count > 0)
                    {
                        chunkMeshes.Add(new(textureName: this.textureName, vertList: newVerticesArray[blockNoX, blockNoY], indicesList: newIndicesArray[blockNoX, blockNoY], drawPriority: this.drawPriority));
                    }
                }
            }

            return chunkMeshes;
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
        public readonly Mesh[,][] meshArray;

        public MeshGrid(int totalWidth, int totalHeight, int blockWidth, int blockHeight, Mesh[] inputMeshArray)
        {
            if (totalWidth <= 0 || totalWidth <= 0) throw new ArgumentException($"Invalid total size {totalWidth}x{totalWidth}.");
            if (blockWidth <= 0 || blockHeight <= 0) throw new ArgumentException($"Invalid block size {blockWidth}x{blockHeight}.");

            this.totalWidth = totalWidth;
            this.totalHeight = totalHeight;
            this.blockWidth = Math.Min(blockWidth, totalWidth);
            this.blockHeight = Math.Min(blockHeight, totalHeight);

            int numBlocksX = (int)Math.Ceiling((double)totalWidth / (double)blockWidth);
            int numBlocksY = (int)Math.Ceiling((double)totalHeight / (double)blockHeight);

            var meshArray = new Mesh[numBlocksX, numBlocksY][];

            Parallel.For(0, numBlocksX, blockNoX =>
            {
                Rectangle blockRect = new(x: 0, y: 0, width: blockWidth, height: blockHeight);
                blockRect.X = blockNoX * blockWidth;

                for (int blockNoY = 0; blockNoY < numBlocksY; blockNoY++)
                {
                    blockRect.Y = blockNoY * blockHeight;

                    var blockMeshList = new List<Mesh>();
                    foreach (Mesh mesh in inputMeshArray)
                    {
                        if (mesh.boundsRect.Intersects(blockRect)) blockMeshList.Add(mesh);
                    }

                    meshArray[blockNoX, blockNoY] = blockMeshList.ToArray();
                }
            });

            this.numBlocksX = numBlocksX;
            this.numBlocksY = numBlocksY;
            this.meshArray = meshArray;
        }

        public List<Mesh> GetMeshesForRect(Rectangle rect)
        {
            List<Mesh> meshesInRect = new();

            // Calculate the block range that overlaps with the given rectangle.
            int startBlockX = Math.Max(0, rect.Left / blockWidth);
            int endBlockX = Math.Min(this.meshArray.GetLength(0) - 1, rect.Right / blockWidth);
            int startBlockY = Math.Max(0, rect.Top / blockHeight);
            int endBlockY = Math.Min(this.meshArray.GetLength(1) - 1, rect.Bottom / blockHeight);

            for (int blockNoX = startBlockX; blockNoX <= endBlockX; blockNoX++)
            {
                for (int blockNoY = startBlockY; blockNoY <= endBlockY; blockNoY++)
                {
                    meshesInRect.AddRange(meshArray[blockNoX, blockNoY]);
                }
            }

            return meshesInRect;
        }
    }
}