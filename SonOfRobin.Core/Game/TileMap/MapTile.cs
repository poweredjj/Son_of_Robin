namespace SonOfRobin
{
    public class MapTile
    {
        public readonly string id;
        public readonly int x;
        public readonly int y;
        public readonly MapTileData mapTileData;

        public MapTile(int x, int y, MapTileData mapTileData)
        {
            this.id = Helpers.GetUniqueHash();
            this.x = x;
            this.y = y;
            this.mapTileData = mapTileData;
        }

    }
}
