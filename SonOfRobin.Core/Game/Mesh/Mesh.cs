using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public readonly struct Mesh
    {
        public const float currentVersion = 1.018f;

        public readonly string meshID;
        public readonly TextureBank.TextureName textureName;
        private readonly VertexPositionTexture[] vertices;
        private readonly VertexPositionTexture[] verticesTransformedCopy;
        public readonly short[] indices;
        public readonly int triangleCount;
        public readonly Rectangle boundsRect;
        public readonly MeshDefinition meshDef;

        public Mesh(TextureBank.TextureName textureName, VertexPositionTexture[] vertArray, short[] indicesArray)
        {
            this.textureName = textureName;
            this.vertices = vertArray;
            this.verticesTransformedCopy = new VertexPositionTexture[vertArray.Length];
            Array.Copy(sourceArray: vertArray, destinationArray: this.verticesTransformedCopy, length: vertArray.Length);
            this.indices = indicesArray;
            this.triangleCount = this.indices.Length / 3;
            this.boundsRect = GetBoundsRect(vertices);
            this.meshID = GetID(boundsRect: boundsRect, textureName: textureName);
            this.meshDef = MeshDefinition.meshDefByTextureName[this.textureName];
        }

        private static string GetID(Rectangle boundsRect, TextureBank.TextureName textureName)
        {
            // needed to filter out duplicates
            return $"{boundsRect.Left},{boundsRect.Top}_{boundsRect.Width}x{boundsRect.Height}_{textureName}";
        }

        public Mesh(object meshData)
        {
            var meshDict = (Dictionary<string, Object>)meshData;

            this.textureName = (TextureBank.TextureName)(Int64)meshDict["textureName"];

            this.indices = (short[])meshDict["indices"];
            this.triangleCount = this.indices.Length / 3;

            this.boundsRect = (Rectangle)meshDict["boundsRect"];
            this.meshID = GetID(boundsRect: boundsRect, textureName: textureName);

            var vertXPosAsSpan = ((float[])meshDict["vertXPos"]).AsSpan();
            var vertYPosAsSpan = ((float[])meshDict["vertYPos"]).AsSpan();
            var vertTexCoordXAsSpan = ((float[])meshDict["vertTexCoordX"]).AsSpan();
            var vertTexCoordYAsSpan = ((float[])meshDict["vertTexCoordY"]).AsSpan();

            this.vertices = new VertexPositionTexture[vertXPosAsSpan.Length];

            Span<VertexPositionTexture> verticesAsSpan = this.vertices.AsSpan();

            for (int i = 0; i < vertXPosAsSpan.Length; i++)
            {
                verticesAsSpan[i] = new()
                {
                    Position = new Vector3(vertXPosAsSpan[i], vertYPosAsSpan[i], 0),
                    TextureCoordinate = new Vector2(vertTexCoordXAsSpan[i], vertTexCoordYAsSpan[i])
                };
            }

            this.verticesTransformedCopy = new VertexPositionTexture[this.vertices.Length];
            Array.Copy(sourceArray: this.vertices, destinationArray: this.verticesTransformedCopy, length: this.vertices.Length);

            this.meshDef = MeshDefinition.meshDefByTextureName[this.textureName];
        }

        public Object Serialize()
        {
            Span<VertexPositionTexture> verticesAsSpan = this.vertices.AsSpan();
            int verticesCount = verticesAsSpan.Length;

            float[] vertXPos = new float[verticesCount];
            float[] vertYPos = new float[verticesCount];
            float[] vertTexCoordX = new float[verticesCount];
            float[] vertTexCoordY = new float[verticesCount];

            var vertXPosAsSpan = vertXPos.AsSpan();
            var vertYPosAsSpan = vertYPos.AsSpan();
            var vertTexCoordXAsSpan = vertTexCoordX.AsSpan();
            var vertTexCoordYAsSpan = vertTexCoordY.AsSpan();

            for (int i = 0; i < verticesAsSpan.Length; i++)
            {
                VertexPositionTexture vertex = verticesAsSpan[i];

                vertXPosAsSpan[i] = vertex.Position.X;
                vertYPosAsSpan[i] = vertex.Position.Y;
                vertTexCoordXAsSpan[i] = vertex.TextureCoordinate.X;
                vertTexCoordYAsSpan[i] = vertex.TextureCoordinate.Y;
            }

            Dictionary<string, Object> meshData = new()
            {
                { "textureName", this.textureName },
                { "indices", this.indices },
                { "boundsRect", this.boundsRect },
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

            Span<VertexPositionTexture> vertArrayAsSpan = vertArray.AsSpan();

            for (int i = 0; i < vertArrayAsSpan.Length; i++)
            {
                VertexPositionTexture vertex = vertArrayAsSpan[i];

                if (vertex.Position.X < xMin) xMin = (int)vertex.Position.X;
                if (vertex.Position.X > xMax) xMax = (int)vertex.Position.X;
                if (vertex.Position.Y < yMin) yMin = (int)vertex.Position.Y;
                if (vertex.Position.Y > yMax) yMax = (int)vertex.Position.Y;
            }

            return new Rectangle(x: xMin, y: yMin, width: xMax - xMin, height: yMax - yMin);
        }

        public void Draw(bool processTweeners)
        {
            if (processTweeners)
            {
                if (this.meshDef.TweenerActive)
                {
                    Vector2 deformationOffset = Vector2.Zero;

                    Span<VertexPositionTexture> verticesAsSpan = this.vertices.AsSpan();
                    Span<VertexPositionTexture> verticesTransformedCopyAsSpan = this.verticesTransformedCopy.AsSpan();

                    for (int i = 0; i < verticesAsSpan.Length; i++)
                    {
                        VertexPositionTexture inputVertex = verticesAsSpan[i];

                        if (meshDef.textureDeformationOffsetX != 0) deformationOffset.X = (float)Math.Sin(inputVertex.TextureCoordinate.X * 3) * meshDef.textureDeformationOffsetX;
                        if (meshDef.textureDeformationOffsetY != 0) deformationOffset.Y = (float)Math.Cos(inputVertex.TextureCoordinate.Y * 3) * meshDef.textureDeformationOffsetY;

                        verticesTransformedCopyAsSpan[i].TextureCoordinate = inputVertex.TextureCoordinate + this.meshDef.TextureOffset + deformationOffset;
                    }
                }
            }

            SonOfRobinGame.GfxDev.DrawUserIndexedPrimitives<VertexPositionTexture>(
                PrimitiveType.TriangleList, this.meshDef.TweenerActive ? this.verticesTransformedCopy : this.vertices, 0, this.vertices.Length, indices, 0, this.triangleCount);
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
        public readonly Mesh[] allMeshes;

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

            this.allMeshes = inputMeshArray;
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
                    meshesInRect.AddRange(this.meshArray[blockNoX, blockNoY]);
                }
            }

            return meshesInRect; // will output duplicates
        }
    }
}