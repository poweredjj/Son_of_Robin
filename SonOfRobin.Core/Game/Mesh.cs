using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SonOfRobin
{
    public readonly struct Mesh
    {
        public readonly Texture2D texture;
        public readonly VertexPositionTexture[] vertArray;
        public readonly short[] indices;
        public readonly int triangleCount;
        public readonly Rectangle boundsRect;

        public Mesh(Texture2D texture, List<VertexPositionTexture> vertList, short[] indices)
        {
            var vertArray = vertList.ToArray();

            this.texture = texture;
            this.vertArray = vertArray;
            this.indices = indices;
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
    }
}