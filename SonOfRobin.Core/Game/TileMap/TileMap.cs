﻿using DeBroglie;
using DeBroglie.Constraints;
using DeBroglie.Models;
using DeBroglie.Topo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class TileMap
    {
        public enum MapType { DefaultOverworld, Test1, Test2 }

        public readonly MapType mapType;
        public readonly int width;
        public readonly int height;
        public readonly int seed;
        public readonly Random random;
        private readonly bool showProgressBar;

        private readonly List<MapTile> allTiles;
        private readonly Dictionary<MapTileData.Name, Tile> tileByName;
        public readonly MapTile[,] tileBoard;

        private readonly DateTime creationTime;
        private TimeSpan processTime;

        public readonly int allElementsCount;
        public bool ReadyToUse { get; private set; }

        private AdjacentModel model;
        private GridTopology topology;
        private TilePropagator propagator;
        private ITopoArray<MapTileData.Name> propagatorOutput;

        public TileMap(MapType mapType, int width, int height, int seed = -1, bool showProgressBar = false)
        {
            this.creationTime = DateTime.Now;
            this.ReadyToUse = false;

            this.mapType = mapType;
            this.width = width;
            this.height = height;
            this.showProgressBar = showProgressBar;
            this.allElementsCount = this.width * this.height;

            this.tileBoard = new MapTile[this.width, this.height];
            this.allTiles = new List<MapTile>();

            this.seed = seed == -1 ? new Random().Next(9999) : seed;
            this.random = new Random(this.seed);

            this.model = new AdjacentModel(directions: DirectionSet.Cartesian2d);

            List<MapTileData.Name> tileNameBlackList = this.GetBlackList();
            List<MapTileData.Name> tileNameWhiteList = this.GetWhiteList();
            this.tileByName = MapTileData.CreateDeBroglieTiles(nameBlackList: tileNameBlackList, nameWhiteList: tileNameWhiteList);
            this.topology = new GridTopology(width: width, height: height, periodic: false);

            MapTileData.SetAdjacency(model: this.model, tileByName: this.tileByName);

            TilePropagatorOptions tilePropagatorOptions = new TilePropagatorOptions
            {
                RandomDouble = random.NextDouble,
                BackTrackDepth = 1000,
                Constraints = this.GetConstrains(),
            };

            this.propagator = new TilePropagator(tileModel: model, topology: topology, options: tilePropagatorOptions);
            this.propagatorOutput = null;

            var xList = new List<int> { this.width / 4, (this.width / 4) + (this.width / 2) };
            var yList = new List<int> { this.height / 4, (this.height / 4) + (this.height / 2) };

            foreach (int x in xList)
            {
                foreach (int y in yList)
                {
                    // this.propagator.Select(x: x, y: y, z: 0, tile: this.tileByName[MapTileData.Name.Dirt]);
                }
            }

            this.ShowProgressBar();

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Creating tile map {this.width}x{this.height} seed {this.seed}.", color: Color.LightCyan);
        }

        private List<MapTileData.Name> GetWhiteList()
        {
            switch (this.mapType)
            {
                case MapType.DefaultOverworld:
                    return new List<MapTileData.Name> { };

                case MapType.Test1:
                    return new List<MapTileData.Name> { };

                case MapType.Test2:
                    return new List<MapTileData.Name> { };

                default:
                    throw new DivideByZeroException($"Unsupported mapType - {this.mapType}.");
            }
        }

        private List<MapTileData.Name> GetBlackList()
        {
            switch (this.mapType)
            {
                case MapType.DefaultOverworld:
                    return new List<MapTileData.Name> { MapTileData.Name.Empty };

                case MapType.Test1:
                    return new List<MapTileData.Name> { MapTileData.Name.Empty };

                case MapType.Test2:
                    return new List<MapTileData.Name> { MapTileData.Name.Empty };

                default:
                    throw new DivideByZeroException($"Unsupported mapType - {this.mapType}.");
            }
        }
        private ITileConstraint[] GetConstrains()
        {
            var constrainsList = new List<ITileConstraint> { };

            switch (this.mapType)
            {
                case MapType.DefaultOverworld:

                    constrainsList.Add(new BorderConstraint
                    {
                        Tiles = new Tile[] { this.tileByName[MapTileData.Name.DeepWater] },
                        Sides = BorderSides.XMin,
                    });

                    var deepWaterTilesList = this.GetTilesContaining(name: MapTileData.Name.DeepWater, bottomName: MapTileData.Name.DeepWater, topName: MapTileData.Name.DeepWater);
                    var deepWaterTilesSet = new HashSet<Tile>();
                    foreach (Tile tile in deepWaterTilesList)
                    {
                        deepWaterTilesSet.Add(tile);
                    }



                    //constrainsList.Add(new CountConstraint
                    //{
                    //    Count = (int)(this.allElementsCount * 0.03f),
                    //    Comparison = CountComparison.AtLeast,
                    //    Tiles = tilesContainingSet,
                    //});

                    //constrainsList.Add(new CountConstraint
                    //{
                    //    Count = (int)(this.allElementsCount * 0.25f),
                    //    Comparison = CountComparison.AtMost,
                    //    Tiles = tilesContainingSet,
                    //});

                    //constrainsList.Add(new MaxConsecutiveConstraint
                    //{
                    //    Tiles = deepWaterTilesSet,
                    //    MaxCount = 10,
                    //});

                    var xList = new List<int> { this.width / 4, (this.width / 4) + (this.width / 2) };
                    var yList = new List<int> { this.height / 4, (this.height / 4) + (this.height / 2) };
                    foreach (int x in xList)
                    {
                        foreach (int y in yList)
                        {
                            constrainsList.Add(new FixedTileConstraint
                            {
                                Tiles = new Tile[] { this.tileByName[MapTileData.Name.Dirt] },
                                Point = new DeBroglie.Point(x, y)
                            });
                        }
                    }

                    break;

                case MapType.Test1:
                    break;

                case MapType.Test2:
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported mapType - {this.mapType}.");
            }

            return constrainsList.ToArray();
        }


        private List<Tile> GetTilesContaining(MapTileData.Name name = MapTileData.Name.Empty, MapTileData.Name topName = MapTileData.Name.Empty, MapTileData.Name bottomName = MapTileData.Name.Empty)
        {
            return this.tileByName.Where(
                kvp => (kvp.Key == name && name != MapTileData.Name.Empty) ||
                (MapTileData.tileDict[kvp.Key].topName == topName && topName != MapTileData.Name.Empty) ||
                (MapTileData.tileDict[kvp.Key].bottomName == bottomName && bottomName != MapTileData.Name.Empty)

            ).Select(kvp => kvp.Value).ToList();
        }

        public void ProcessNextGeneratorStep(int processCount = 1)
        {
            SonOfRobinGame.game.IsFixedTimeStep = false;

            for (int i = 0; i < processCount; i++)
            {
                this.propagatorOutput = null;

                Resolution resolution = this.propagator.Step();

                if (resolution == Resolution.Decided)
                {
                    this.FinishProcessing();
                    return;
                }
            }

            this.ShowProgressBar();
        }

        private void FinishProcessing()
        {
            this.processTime = DateTime.Now - this.creationTime;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Tile map  {this.width}x{this.height} seed {this.seed} procesing time: {this.processTime:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

            SonOfRobinGame.progressBar.TurnOff();
            SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;

            this.propagatorOutput = this.propagator.ToValueArray<MapTileData.Name>();

            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    MapTileData tileData = MapTileData.GetTile(this.propagatorOutput.Get(x, y));
                    MapTile tile = new MapTile(x: x, y: y, mapTileData: tileData);

                    this.allTiles.Add(tile);
                    this.tileBoard[x, y] = tile;
                }
            }

            this.model = null;
            this.topology = null;
            this.propagator = null;
            this.propagatorOutput = null;

            this.ReadyToUse = true;
        }

        public MapTile GetTile(int x, int y)
        {
            return this.tileBoard[x, y];
        }

        public Texture2D GetTileTexture(int x, int y)
        {
            if (this.ReadyToUse) return this.tileBoard[x, y].mapTileData.texture;
            else
            {
                if (this.propagatorOutput == null) this.propagatorOutput = propagator.ToValueArray<MapTileData.Name>();

                MapTileData mapTile = MapTileData.GetTile(this.propagatorOutput.Get(x, y));
                return mapTile.texture;
            }
        }

        private void ShowProgressBar()
        {
            if (!this.showProgressBar) return;

            int completeAmount = (int)(this.propagator.GetProgress() * (double)this.allElementsCount);


            TimeSpan timeLeft = CalculateTimeLeft(startTime: this.creationTime, completeAmount: completeAmount, totalAmount: this.allElementsCount);
            string timeLeftString = TimeSpanToString(timeLeft + TimeSpan.FromSeconds(1));

            string seedText = String.Format("{0:0000}", this.seed);
            string message = $"preparing map\nseed {seedText}\n{this.width} x {this.height}\ngenerating terrain {timeLeftString}";

            SonOfRobinGame.progressBar.TurnOn(curVal: completeAmount, maxVal: this.allElementsCount, text: message);
        }

        private static TimeSpan CalculateTimeLeft(DateTime startTime, int completeAmount, int totalAmount)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            try
            {
                TimeSpan timePerCell = TimeSpan.FromSeconds(elapsedTime.TotalSeconds / completeAmount);
                TimeSpan timeLeft = TimeSpan.FromSeconds(timePerCell.TotalSeconds * (totalAmount - completeAmount));

                return timeLeft;
            }
            catch (OverflowException) { return TimeSpan.FromMinutes(30); }
        }

        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            string timeLeftString;

            if (timeSpan < TimeSpan.FromMinutes(1)) timeLeftString = timeSpan.ToString("ss");
            else if (timeSpan < TimeSpan.FromHours(1)) timeLeftString = timeSpan.ToString("mm\\:ss");
            else if (timeSpan < TimeSpan.FromDays(1)) timeLeftString = timeSpan.ToString("hh\\:mm\\:ss");
            else timeLeftString = timeSpan.ToString("dd\\:hh\\:mm\\:ss");

            return timeLeftString;
        }

    }
}
