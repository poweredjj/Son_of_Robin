using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public readonly struct Mesh
    {
        public const float currentVersion = 1.0267f;

        public readonly string meshID;
        public readonly TextureBank.TextureName textureName;
        private readonly VertexPositionTexture[] vertices;
        public readonly short[] indices;
        public readonly int triangleCount;
        public readonly Rectangle boundsRect;

        public Mesh(TextureBank.TextureName textureName, VertexPositionTexture[] vertArray, short[] indicesArray)
        {
            this.textureName = textureName;
            this.vertices = vertArray;
            this.indices = indicesArray;
            this.triangleCount = this.indices.Length / 3;
            this.boundsRect = GetBoundsRect(vertices);
            this.meshID = GetID(boundsRect: boundsRect, textureName: textureName);
        }

        public MeshDefinition MeshDef { get { return MeshDefinition.meshDefByTextureName[this.textureName]; } }

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

        public void Draw()
        {
            SonOfRobinGame.GfxDev.DrawUserIndexedPrimitives<VertexPositionTexture>(
                PrimitiveType.TriangleList, this.vertices, 0, this.vertices.Length, indices, 0, this.triangleCount);
        }
    }
}