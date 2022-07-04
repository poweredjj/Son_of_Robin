using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public struct MapTile
    {
        public enum Name
        {
            Sand, Grass, SandOnGrassLT, SandOnGrassRT, SandOnGrassLB, SandOnGrassRB,
        }

        public static readonly Dictionary<Name, MapTile> tileDict = new Dictionary<Name, MapTile>();

        public readonly Name name;
        public readonly Texture2D texture;
        public readonly bool edge;

        public MapTile(Name name, Texture2D texture, bool edge)
        {
            this.name = name;
            this.texture = texture;
            this.edge = edge;

            if (tileDict.ContainsKey(name)) throw new ArgumentException($"Tile '{name}' has already been defined.");
            tileDict[name] = this;
        }


        public static MapTile GetTile(Name name)
        {
            return tileDict[name];
        }

        public static void CreateAllTiles()
        {
            if (tileDict.Count > 0) throw new ArgumentException("Tiles already created.");

            new MapTile(name: Name.Sand, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSand].texture, edge: false);
            new MapTile(name: Name.Grass, texture: AnimData.framesForPkgs[AnimData.PkgName.TileGrass].texture, edge: false);
            new MapTile(name: Name.SandOnGrassLT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLT].texture, edge: true);
            new MapTile(name: Name.SandOnGrassRT, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRT].texture, edge: true);
            new MapTile(name: Name.SandOnGrassLB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassLB].texture, edge: true);
            new MapTile(name: Name.SandOnGrassRB, texture: AnimData.framesForPkgs[AnimData.PkgName.TileSandOnGrassRB].texture, edge: true);
        }

    }
}
