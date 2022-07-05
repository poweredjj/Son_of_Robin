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
        public enum Name { Sand, Grass, Water, SandOnGrassLT, SandOnGrassRT, SandOnGrassLB, SandOnGrassRB, SandOnGrassB, SandOnGrassT, SandOnGrassL, SandOnGrassR, WaterOnSandLT, WaterOnSandRT, WaterOnSandLB, WaterOnSandRB, WaterOnSandB, WaterOnSandT, WaterOnSandL, WaterOnSandR }
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

            MapTile grass = new MapTile(name: Name.Grass, texture: AnimData.framesForPkgs[AnimData.PkgName.TileGrass].texture, edge: false);
            MapTile sand = new MapTile(name: Name.Sand, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSand].texture, edge: false);
            MapTile sandOnGrassLT = new MapTile(name: Name.SandOnGrassLT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLT].texture, edge: true);
            MapTile sandOnGrassRT = new MapTile(name: Name.SandOnGrassRT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRT].texture, edge: true);
            MapTile sandOnGrassLB = new MapTile(name: Name.SandOnGrassLB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLB].texture, edge: true);
            MapTile sandOnGrassRB = new MapTile(name: Name.SandOnGrassRB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRB].texture, edge: true);
            MapTile sandOnGrassT = new MapTile(name: Name.SandOnGrassT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassT].texture, edge: true);
            MapTile sandOnGrassB = new MapTile(name: Name.SandOnGrassB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassB].texture, edge: true);
            MapTile sandOnGrassR = new MapTile(name: Name.SandOnGrassR, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassR].texture, edge: true);
            MapTile sandOnGrassL = new MapTile(name: Name.SandOnGrassL, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassL].texture, edge: true);

            MapTile water = new MapTile(name: Name.Water, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWater].texture, edge: false);
            MapTile waterOnSandLT = new MapTile(name: Name.WaterOnSandLT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandLT].texture, edge: true);
            MapTile waterOnSandRT = new MapTile(name: Name.WaterOnSandRT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandRT].texture, edge: true);
            MapTile waterOnSandLB = new MapTile(name: Name.WaterOnSandLB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandLB].texture, edge: true);
            MapTile waterOnSandRB = new MapTile(name: Name.WaterOnSandRB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandRB].texture, edge: true);
            MapTile waterOnSandT = new MapTile(name: Name.WaterOnSandT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandT].texture, edge: true);
            MapTile waterOnSandB = new MapTile(name: Name.WaterOnSandB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandB].texture, edge: true);
            MapTile waterOnSandR = new MapTile(name: Name.WaterOnSandR, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandR].texture, edge: true);
            MapTile waterOnSandL = new MapTile(name: Name.WaterOnSandL, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandL].texture, edge: true);

            CreateAllEdgeCombinations(solidBG: grass, solidFG: sand, leftTop: sandOnGrassLT, top: sandOnGrassT, rightTop: sandOnGrassRT, right: sandOnGrassR, rightBottom: sandOnGrassRB, bottom: sandOnGrassB, leftBottom: sandOnGrassLB, left: sandOnGrassL);

            CreateAllEdgeCombinations(solidBG: sand, solidFG: water, leftTop: waterOnSandLT, top: waterOnSandT, rightTop: waterOnSandRT, right: waterOnSandR, rightBottom: waterOnSandRB, bottom: waterOnSandB, leftBottom: waterOnSandLB, left: waterOnSandL);
        }

        private static void CreateAllEdgeCombinations(MapTile solidBG, MapTile solidFG, MapTile leftTop, MapTile top, MapTile rightTop, MapTile right, MapTile rightBottom, MapTile bottom, MapTile leftBottom, MapTile left)
        {
            // solid

            solidBG.AddAllowedNeighbour(name: solidBG.name);
            solidBG.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { leftBottom.name, rightBottom.name });
            solidBG.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { rightBottom.name, rightTop.name });
            solidBG.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { leftBottom.name, leftTop.name });
            solidBG.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { leftTop.name, rightTop.name });

            solidFG.AddAllowedNeighbour(name: solidFG.name);
            solidFG.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { leftTop.name, top.name, rightTop.name });
            solidFG.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { leftTop.name, left.name, leftBottom.name });
            solidFG.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { rightTop.name, right.name, rightBottom.name });
            solidFG.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { leftBottom.name, bottom.name, rightBottom.name });

            // corners

            leftTop.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { solidBG.name });
            leftTop.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { solidBG.name });
            leftTop.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { rightTop.name });
            leftTop.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { leftBottom.name });

            rightTop.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { solidBG.name });
            rightTop.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { leftTop.name });
            rightTop.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { solidBG.name });
            rightTop.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { rightBottom.name });

            leftBottom.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { rightTop.name });
            leftBottom.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { solidBG.name });
            leftBottom.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { rightBottom.name });
            leftBottom.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { solidBG.name });

            rightBottom.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { rightTop.name });
            rightBottom.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { leftBottom.name });
            rightBottom.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { solidBG.name });
            rightBottom.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { solidBG.name });

            // sides

            top.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { solidBG.name });
            top.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { left.name, leftTop.name });
            top.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { right.name, rightTop.name });
            top.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { solidFG.name, bottom.name });

            bottom.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { solidFG.name, top.name });
            bottom.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { left.name, leftBottom.name });
            bottom.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { right.name, rightBottom.name });
            bottom.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { solidBG.name });

            right.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { rightTop.name, right.name });
            right.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { solidFG.name, left.name });
            right.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { solidBG.name });
            right.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { right.name, rightBottom.name });

            left.AddAllowedNeighbour(direction: Direction.Up, nameList: new List<Name> { left.name, leftTop.name });
            left.AddAllowedNeighbour(direction: Direction.Left, nameList: new List<Name> { solidBG.name });
            left.AddAllowedNeighbour(direction: Direction.Right, nameList: new List<Name> { solidFG.name, right.name });
            left.AddAllowedNeighbour(direction: Direction.Down, nameList: new List<Name> { left.name, leftBottom.name });
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
