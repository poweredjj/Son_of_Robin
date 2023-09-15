using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public readonly struct Mesh
    {
        public const float currentVersion = 1.003f;

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
            this.boundsRect = GetBoundsRect(vertices);
            this.drawPriority = drawPriority;
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
                { "drawPriority", drawPriority },
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
}