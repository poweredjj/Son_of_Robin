using DeBroglie.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public struct NeighbourData
    {
        public readonly TileData.Name name;
        public readonly int xOffset;
        public readonly int yOffset;
        public NeighbourData(TileData.Name name, int xOffset, int yOffset)
        {
            this.name = name;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
        }
    }

    public struct TileData
    {
        public enum Name
        {
            Empty,
            Sand, Grass, Water, Dirt,
            SandOnGrassLT, SandOnGrassRT, SandOnGrassLB, SandOnGrassRB, SandOnGrassB, SandOnGrassT, SandOnGrassL, SandOnGrassR,
            WaterOnSandLT, WaterOnSandRT, WaterOnSandLB, WaterOnSandRB, WaterOnSandB, WaterOnSandT, WaterOnSandL, WaterOnSandR,
            DirtOnGrassLT, DirtOnGrassRT, DirtOnGrassLB, DirtOnGrassRB, DirtOnGrassB, DirtOnGrassT, DirtOnGrassL, DirtOnGrassR,
        }
        public enum Direction { Left, Right, Up, Down }

        public static readonly Dictionary<Direction, Vector2> vectorForDirection = new Dictionary<Direction, Vector2>
        {
            { Direction.Left, new Vector2(-1, 0) },
            { Direction.Right, new Vector2(1, 0) },
            { Direction.Up, new Vector2(0, -1) },
            { Direction.Down, new Vector2(0, 1) },
        };

        public static readonly Dictionary<Name, TileData> tileDict = new Dictionary<Name, TileData>();

        public readonly Name name;
        public readonly Texture2D texture;
        public readonly double frequency;
        public readonly bool edge;
        public readonly List<NeighbourData> allowedNeighbours;

        public TileData(Name name, Texture2D texture, bool edge, double frequency = 1d)
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

        public static TileData GetTile(Name name)
        {
            return tileDict[name];
        }

        public static void CreateAllTiles()
        {
            if (tileDict.Count > 0) throw new ArgumentException("Tiles already created.");

            TileData empty = new TileData(name: Name.Empty, texture: SonOfRobinGame.whiteRectangle, edge: false); // to see uncollapsed tiles in their initial state

            TileData grass = new TileData(name: Name.Grass, texture: AnimData.framesForPkgs[AnimData.PkgName.TileGrass].texture, edge: false);
            TileData sand = new TileData(name: Name.Sand, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSand].texture, edge: false);
            TileData water = new TileData(name: Name.Water, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWater].texture, edge: false);
            TileData dirt = new TileData(name: Name.Dirt, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirt].texture, edge: false);

            TileData sandOnGrassLT = new TileData(name: Name.SandOnGrassLT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLT].texture, edge: true);
            TileData sandOnGrassRT = new TileData(name: Name.SandOnGrassRT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRT].texture, edge: true);
            TileData sandOnGrassLB = new TileData(name: Name.SandOnGrassLB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLB].texture, edge: true);
            TileData sandOnGrassRB = new TileData(name: Name.SandOnGrassRB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRB].texture, edge: true);
            TileData sandOnGrassT = new TileData(name: Name.SandOnGrassT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassT].texture, edge: true);
            TileData sandOnGrassB = new TileData(name: Name.SandOnGrassB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassB].texture, edge: true);
            TileData sandOnGrassR = new TileData(name: Name.SandOnGrassR, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassR].texture, edge: true);
            TileData sandOnGrassL = new TileData(name: Name.SandOnGrassL, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassL].texture, edge: true);

            TileData waterOnSandLT = new TileData(name: Name.WaterOnSandLT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandLT].texture, edge: true);
            TileData waterOnSandRT = new TileData(name: Name.WaterOnSandRT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandRT].texture, edge: true);
            TileData waterOnSandLB = new TileData(name: Name.WaterOnSandLB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandLB].texture, edge: true);
            TileData waterOnSandRB = new TileData(name: Name.WaterOnSandRB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandRB].texture, edge: true);
            TileData waterOnSandT = new TileData(name: Name.WaterOnSandT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandT].texture, edge: true);
            TileData waterOnSandB = new TileData(name: Name.WaterOnSandB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandB].texture, edge: true);
            TileData waterOnSandR = new TileData(name: Name.WaterOnSandR, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandR].texture, edge: true);
            TileData waterOnSandL = new TileData(name: Name.WaterOnSandL, texture: AnimData.framesForPkgs[AnimData.PkgName.TileWaterOnSandL].texture, edge: true);

            TileData dirtOnGrassLT = new TileData(name: Name.DirtOnGrassLT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirtOnGrassLT].texture, edge: true);
            TileData dirtOnGrassRT = new TileData(name: Name.DirtOnGrassRT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirtOnGrassRT].texture, edge: true);
            TileData dirtOnGrassLB = new TileData(name: Name.DirtOnGrassLB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirtOnGrassLB].texture, edge: true);
            TileData dirtOnGrassRB = new TileData(name: Name.DirtOnGrassRB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirtOnGrassRB].texture, edge: true);
            TileData dirtOnGrassT = new TileData(name: Name.DirtOnGrassT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirtOnGrassT].texture, edge: true);
            TileData dirtOnGrassB = new TileData(name: Name.DirtOnGrassB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirtOnGrassB].texture, edge: true);
            TileData dirtOnGrassR = new TileData(name: Name.DirtOnGrassR, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirtOnGrassR].texture, edge: true);
            TileData dirtOnGrassL = new TileData(name: Name.DirtOnGrassL, texture: AnimData.framesForPkgs[AnimData.PkgName.TileDirtOnGrassL].texture, edge: true);

            CreateAllEdgeCombinations(solidBG: grass, solidFG: sand, leftTop: sandOnGrassLT, top: sandOnGrassT, rightTop: sandOnGrassRT, right: sandOnGrassR, rightBottom: sandOnGrassRB, bottom: sandOnGrassB, leftBottom: sandOnGrassLB, left: sandOnGrassL);

            CreateAllEdgeCombinations(solidBG: sand, solidFG: water, leftTop: waterOnSandLT, top: waterOnSandT, rightTop: waterOnSandRT, right: waterOnSandR, rightBottom: waterOnSandRB, bottom: waterOnSandB, leftBottom: waterOnSandLB, left: waterOnSandL);

            CreateAllEdgeCombinations(solidBG: grass, solidFG: dirt, leftTop: dirtOnGrassLT, top: dirtOnGrassT, rightTop: dirtOnGrassRT, right: dirtOnGrassR, rightBottom: dirtOnGrassRB, bottom: dirtOnGrassB, leftBottom: dirtOnGrassLB, left: dirtOnGrassL);
        }

        private static void CreateAllEdgeCombinations(TileData solidBG, TileData solidFG, TileData leftTop, TileData top, TileData rightTop, TileData right, TileData rightBottom, TileData bottom, TileData leftBottom, TileData left)
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
            var tileByName = new Dictionary<Name, DeBroglie.Tile>();

            foreach (TileData mapTile in tileDict.Values)
            {
                var tile = new DeBroglie.Tile(mapTile.name);
                model.SetFrequency(tile, mapTile.frequency);
                tileByName[mapTile.name] = tile;
            }

            foreach (var kvp in tileByName)
            {
                Name name = kvp.Key;
                DeBroglie.Tile tile = tileByName[name];
                TileData mapTile = tileDict[name];

                foreach (NeighbourData neighbourData in mapTile.allowedNeighbours)
                {
                    model.AddAdjacency(src: tile, dest: tileByName[neighbourData.name], x: neighbourData.xOffset, y: neighbourData.yOffset, z: 0);
                }
            }
        }

    }
}
