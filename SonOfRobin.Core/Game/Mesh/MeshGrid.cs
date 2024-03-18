using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonOfRobin
{
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
            List<Mesh> meshesInRect = [];

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