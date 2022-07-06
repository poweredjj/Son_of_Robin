using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Tile
    {
        public readonly string id;
        public readonly int x;
        public readonly int y;
        public readonly List<Tile> neighbours;
        public readonly TileData tileData;

        public Tile(int x, int y, TileData tileData)
        {
            this.id = Helpers.GetUniqueHash();
            this.x = x;
            this.y = y;
            this.tileData = tileData;

            this.neighbours = new List<Tile>();
        }

        public void SetNeighbours(List<Tile> neighboursList)
        {
            if (this.neighbours.Any()) throw new ArgumentException("Neighbours list has already been set.");

            this.neighbours.AddRange(neighboursList);
        }


    }
}
