namespace SonOfRobin
{
    public class Tile
    {
        public readonly string id;
        public readonly int x;
        public readonly int y;
        public readonly TileData tileData;

        public Tile(int x, int y, TileData tileData)
        {
            this.id = Helpers.GetUniqueHash();
            this.x = x;
            this.y = y;
            this.tileData = tileData;
        }

    }
}
