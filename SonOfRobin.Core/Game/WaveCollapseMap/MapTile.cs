using DeBroglie;
using DeBroglie.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public struct NeighbourData
    {
        public readonly MapTile.Name name;
        public readonly int xOffset;
        public readonly int yOffset;
        public NeighbourData(MapTile.Name name, int xOffset, int yOffset)
        {
            this.name = name;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
        }
    }

    public struct MapTile
    {
        public enum Name { Sand, Grass, SandOnGrassLT, SandOnGrassRT, SandOnGrassLB, SandOnGrassRB, SandOnGrassCB, SandOnGrassCT, SandOnGrassLC, SandOnGrassRC }
        public enum Direction { Left, Right, Up, Down }

        public static readonly Dictionary<Direction, Vector2> vectorForDirection = new Dictionary<Direction, Vector2>
        {
            { Direction.Left, new Vector2(-1, 0) },
            { Direction.Right, new Vector2(1, 0) },
            { Direction.Up, new Vector2(0, -1) },
            { Direction.Down, new Vector2(0, 1) },
        };

        public static readonly Dictionary<Name, MapTile> tileDict = new Dictionary<Name, MapTile>();

        public readonly Name name;
        public readonly Texture2D texture;
        public readonly double frequency;
        public readonly bool edge;
        public readonly List<NeighbourData> allowedNeighbours;

        public MapTile(Name name, Texture2D texture, bool edge, double frequency = 1d)
        {
            this.name = name;
            this.texture = texture;
            this.edge = edge;
            this.frequency = frequency;
            this.allowedNeighbours = new List<NeighbourData>();

            if (tileDict.ContainsKey(name)) throw new ArgumentException($"Tile '{name}' has already been defined.");
            tileDict[name] = this;
        }

        public void AddAllowedNeighbour(Direction direction, List<Name> nameList)
        {
            Vector2 position = vectorForDirection[direction];
            foreach (Name name in nameList)
            {
                this.allowedNeighbours.Add(new NeighbourData(name: name, xOffset: (int)position.X, (int)position.Y));
            }
        }

        public void AddAllowedNeighbour(Name name)
        {
            foreach (Vector2 position in vectorForDirection.Values)
            {
                this.allowedNeighbours.Add(new NeighbourData(name: name, xOffset: (int)position.X, (int)position.Y));
            }
        }

        public static MapTile GetTile(Name name)
        {
            return tileDict[name];
        }

        public static void CreateAllTiles()
        {
            if (tileDict.Count > 0) throw new ArgumentException("Tiles already created.");

            {
                MapTile tile = new MapTile(name: Name.Grass, texture: AnimData.framesForPkgs[AnimData.PkgName.TileGrass].texture, edge: false);
                tile.AddAllowedNeighbour(name: Name.Grass);
                tile.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { Name.SandOnGrassLB, Name.SandOnGrassRB });
                tile.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { Name.SandOnGrassRB, Name.SandOnGrassRT });
                tile.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { Name.SandOnGrassLB, Name.SandOnGrassLT });
                tile.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { Name.SandOnGrassLT, Name.SandOnGrassRT });
            }

            {
                MapTile tile = new MapTile(name: Name.SandOnGrassLT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLT].texture, edge: true);
                tile.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { Name.Grass });
                tile.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { Name.Grass });
                tile.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { Name.SandOnGrassRT });
                tile.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { Name.SandOnGrassLB });
            }

            {
                MapTile tile = new MapTile(name: Name.SandOnGrassRT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRT].texture, edge: true);
                tile.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { Name.Grass });
                tile.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { Name.SandOnGrassLT });
                tile.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { Name.Grass });
                tile.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { Name.SandOnGrassRB });
            }

            {
                MapTile tile = new MapTile(name: Name.SandOnGrassLB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLB].texture, edge: true);
                tile.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { Name.SandOnGrassRT });
                tile.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { Name.Grass });
                tile.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { Name.SandOnGrassRB });
                tile.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { Name.Grass });
            }

            {
                MapTile tile = new MapTile(name: Name.SandOnGrassRB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRB].texture, edge: true);
                tile.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { Name.SandOnGrassRT });
                tile.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { Name.SandOnGrassLB });
                tile.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { Name.Grass });
                tile.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { Name.Grass });
            }

            {
                MapTile tile = new MapTile(name: Name.Sand, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSand].texture, edge: false);
                tile.AddAllowedNeighbour(name: Name.Sand);
                tile.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { Name.SandOnGrassLB, Name.SandOnGrassCB, Name.SandOnGrassRB });
                tile.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { Name.SandOnGrassRT, Name.SandOnGrassRC, Name.SandOnGrassRB });
                tile.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { Name.SandOnGrassLT, Name.SandOnGrassLC, Name.SandOnGrassLB });
                tile.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { Name.SandOnGrassLT, Name.SandOnGrassCT, Name.SandOnGrassRT });
            }

            new MapTile(name: Name.SandOnGrassCB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassCB].texture, edge: true);
            new MapTile(name: Name.SandOnGrassCT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassCT].texture, edge: true);
            new MapTile(name: Name.SandOnGrassLC, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLC].texture, edge: true);
            new MapTile(name: Name.SandOnGrassRC, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRC].texture, edge: true);


            // TODO add center tiles neighbours
        }

        public static void SetAdjacency(AdjacentModel model)
        {
            var tileByName = new Dictionary<Name, Tile>();

            foreach (MapTile mapTile in tileDict.Values)
            {
                var tile = new Tile(mapTile.name);
                model.SetFrequency(tile, mapTile.frequency);
                tileByName[mapTile.name] = tile;
            }

            foreach (var kvp in tileByName)
            {
                Name name = kvp.Key;
                Tile tile = tileByName[name];
                MapTile mapTile = tileDict[name];

                foreach (NeighbourData neighbourData in mapTile.allowedNeighbours)
                {
                    model.AddAdjacency(src: tile, dest: tileByName[neighbourData.name], x: neighbourData.xOffset, y: neighbourData.yOffset, z: 0);
                }
            }
        }

    }
}
