﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public readonly struct Mesh
    {
        public const float currentVersion = 1.008f;

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
            // meshID cannot use Helpers.GetUniqueID() because it is not thread-safe (duplicated IDs will occur)
            this.meshID = $"{this.boundsRect.Left},{this.boundsRect.Top}_{this.boundsRect.Width}x{this.boundsRect.Height}_{this.textureName}";
        }

        public Mesh(object meshData)
        {
            var meshDict = (Dictionary<string, Object>)meshData;

            this.textureName = (string)meshDict["textureName"];
            this.texture = TextureBank.GetTexture(this.textureName);

            this.indices = (short[])meshDict["indices"];
            this.triangleCount = this.indices.Length / 3;

            this.boundsRect = (Rectangle)meshDict["boundsRect"];
            this.meshID = (string)meshDict["meshID"];

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
                { "meshID", this.meshID },
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
            if (maxChunkSize <= 0) throw new ArgumentException($"Invalid chunk size {maxChunkSize}.");

            int numChunksX = (int)Math.Ceiling((double)this.boundsRect.Width / (double)maxChunkSize);
            int numChunksY = (int)Math.Ceiling((double)this.boundsRect.Height / (double)maxChunkSize);

            if (numChunksX == 1 && numChunksY == 1) return new List<Mesh> { this };

            return new List<Mesh> { this }; // for testing

            var indicesByVertices = new Dictionary<VertexPositionTexture, List<short>>[numChunksX, numChunksY];

            for (int blockNoX = 0; blockNoX < numChunksX; blockNoX++)
            {
                for (int blockNoY = 0; blockNoY < numChunksY; blockNoY++)
                {
                    indicesByVertices[blockNoX, blockNoY] = new Dictionary<VertexPositionTexture, List<short>>();
                }
            }

            // iterating over triangles
            for (int i = 0; i < this.indices.Length; i += 3)
            {
                short index1 = indices[i];
                short index2 = indices[i + 1];
                short index3 = indices[i + 2];

                VertexPositionTexture vertex1 = this.vertices[index1];
                VertexPositionTexture vertex2 = this.vertices[index2];
                VertexPositionTexture vertex3 = this.vertices[index3];

                float xMin = Math.Min(Math.Min(vertex1.Position.X, vertex2.Position.X), vertex3.Position.X);
                float yMin = Math.Min(Math.Min(vertex1.Position.Y, vertex2.Position.Y), vertex3.Position.Y);

                int xBlockNo = (int)((xMin - this.boundsRect.X) / maxChunkSize);
                int yBlockNo = (int)((yMin - this.boundsRect.Y) / maxChunkSize);

                // Add the indices of the triangle to the corresponding vertex positions
                AddToSplitIndices(indicesByVertices[xBlockNo, yBlockNo], vertex1, index1);
                AddToSplitIndices(indicesByVertices[xBlockNo, yBlockNo], vertex2, index2);
                AddToSplitIndices(indicesByVertices[xBlockNo, yBlockNo], vertex3, index3);
            }

            var chunkMeshes = new List<Mesh>();

            for (int blockNoX = 0; blockNoX < numChunksX; blockNoX++)
            {
                for (int blockNoY = 0; blockNoY < numChunksY; blockNoY++)
                {
                    if (indicesByVertices[blockNoX, blockNoY].Count > 0)
                    {
                        var newVertices = new List<VertexPositionTexture>();
                        var newIndices = new List<short>();

                        foreach (var kvp in indicesByVertices[blockNoX, blockNoY])
                        {
                            VertexPositionTexture position = kvp.Key;
                            List<short> indices = kvp.Value;

                            // Add the position to the newVertices list
                            newVertices.Add(position);

                            // Update the indices in the newIndices list to point to the corresponding positions
                            for (int i = 0; i < indices.Count; i++)
                            {
                                short newIndex = (short)(newVertices.Count - 1); // Index of the added position
                                newIndices.Add(newIndex);
                            }
                        }

                        chunkMeshes.Add(new(textureName: this.textureName, vertList: newVertices, indicesList: newIndices, drawPriority: this.drawPriority));
                    }
                }
            }

            return chunkMeshes;
        }

        private static void AddToSplitIndices(Dictionary<VertexPositionTexture, List<short>> splitIndices, VertexPositionTexture vertex, short index)
        {
            if (!splitIndices.ContainsKey(vertex)) splitIndices[vertex] = new List<short>();
            splitIndices[vertex].Add(index);
        }
    }

    public class MeshGrid
    {
        public readonly int totalWidth;
        public readonly int totalHeight;
        public readonly int blockWidth;
        public readonly int blockHeight;
        public readonly int numBlocksX;
        public readonly int numBlocksY;
        public readonly List<Mesh>[,] meshListGrid;

        public MeshGrid(int totalWidth, int totalHeight, int blockWidth, int blockHeight, Mesh[] meshArray)
        {
            if (totalWidth <= 0 || totalWidth <= 0) throw new ArgumentException($"Invalid total size {totalWidth}x{totalWidth}.");
            if (blockWidth <= 0 || blockHeight <= 0) throw new ArgumentException($"Invalid block size {blockWidth}x{blockHeight}.");

            this.totalWidth = totalWidth;
            this.totalHeight = totalHeight;
            this.blockWidth = Math.Min(blockWidth, totalWidth);
            this.blockHeight = Math.Min(blockHeight, totalHeight);

            this.numBlocksX = (int)Math.Ceiling((double)totalWidth / (double)blockWidth);
            this.numBlocksY = (int)Math.Ceiling((double)totalHeight / (double)blockHeight);

            this.meshListGrid = new List<Mesh>[this.numBlocksX, this.numBlocksY];

            Rectangle blockRect = new(x: 0, y: 0, width: blockWidth, height: blockHeight);

            for (int blockNoX = 0; blockNoX < this.numBlocksX; blockNoX++)
            {
                for (int blockNoY = 0; blockNoY < this.numBlocksY; blockNoY++)
                {
                    blockRect.X = blockNoX * blockWidth;
                    blockRect.Y = blockNoY * blockHeight;

                    var blockMeshList = new List<Mesh>();
                    meshListGrid[blockNoX, blockNoY] = blockMeshList;

                    foreach (Mesh mesh in meshArray)
                    {
                        if (mesh.boundsRect.Intersects(blockRect)) blockMeshList.Add(mesh);
                    }
                }
            }
        }

        public MeshGrid(object meshGridData, List<Mesh> meshList)
        {
            var meshGridDict = (Dictionary<string, Object>)meshGridData;

            this.totalWidth = (int)(Int64)meshGridDict["totalWidth"];
            this.totalHeight = (int)(Int64)meshGridDict["totalHeight"];
            this.blockWidth = (int)(Int64)meshGridDict["blockWidth"];
            this.blockHeight = (int)(Int64)meshGridDict["blockHeight"];
            this.numBlocksX = (int)(Int64)meshGridDict["numBlocksX"];
            this.numBlocksY = (int)(Int64)meshGridDict["numBlocksY"];
            var meshIDByBlocks = (List<string>[,])meshGridDict["meshIDByBlocks"];

            var meshByID = meshList.ToDictionary(mesh => mesh.meshID, mesh => mesh);

            this.meshListGrid = new List<Mesh>[this.numBlocksX, this.numBlocksY];

            for (int blockNoX = 0; blockNoX < this.numBlocksX; blockNoX++)
            {
                for (int blockNoY = 0; blockNoY < this.numBlocksY; blockNoY++)
                {
                    var blockMeshList = new List<Mesh>();
                    this.meshListGrid[blockNoX, blockNoY] = blockMeshList;

                    foreach (string meshID in meshIDByBlocks[blockNoX, blockNoY])
                    {
                        blockMeshList.Add(meshByID[meshID]);
                    }
                }
            }
        }

        public Object Serialize()
        {
            var meshIDByBlocks = new List<string>[this.numBlocksX, this.numBlocksY];

            for (int blockNoX = 0; blockNoX < this.numBlocksX; blockNoX++)
            {
                for (int blockNoY = 0; blockNoY < this.numBlocksY; blockNoY++)
                {
                    var blockMeshList = this.meshListGrid[blockNoX, blockNoY];
                    var meshIDList = new List<string>();
                    meshIDByBlocks[blockNoX, blockNoY] = meshIDList;

                    foreach (Mesh mesh in blockMeshList)
                    {
                        meshIDList.Add(mesh.meshID);
                    }
                }
            }

            Dictionary<string, Object> meshData = new()
            {
                { "totalWidth", this.totalWidth },
                { "totalHeight", this.totalHeight },
                { "blockWidth", this.blockWidth },
                { "blockHeight", this.blockHeight },
                { "numBlocksX", this.numBlocksX },
                { "numBlocksY", this.numBlocksY },
                { "meshIDByBlocks", meshIDByBlocks },
            };

            return meshData;
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