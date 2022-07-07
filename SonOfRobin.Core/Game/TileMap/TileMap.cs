using DeBroglie;
using DeBroglie.Models;
using DeBroglie.Topo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class TileMap
    {
        public readonly int width;
        public readonly int height;
        public readonly int seed;
        public readonly Random random;
        private readonly bool showProgressBar;

        private readonly List<Tile> allTiles;
        public readonly Tile[,] tileBoard;

        private readonly DateTime creationTime;
        private TimeSpan processTime;

        public readonly int allElementsCount;
        public bool ReadyToUse { get; private set; }

        private AdjacentModel model;
        private GridTopology topology;
        private TilePropagator propagator;
        private ITopoArray<TileData.Name> propagatorOutput;


        public TileMap(int width, int height, int seed = -1, bool showProgressBar = false)
        {
            this.creationTime = DateTime.Now;
            this.ReadyToUse = false;

            this.width = width;
            this.height = height;
            this.showProgressBar = showProgressBar;
            this.allElementsCount = this.width * this.height;

            this.tileBoard = new Tile[this.width, this.height];
            this.allTiles = new List<Tile>();

            this.seed = seed == -1 ? new Random().Next(9999) : seed;
            this.random = new Random(this.seed);

            this.model = new AdjacentModel(directions: DirectionSet.Cartesian2d);
            TileData.SetAdjacency(this.model);
            this.topology = new GridTopology(width: width, height: height, periodic: false);

            TilePropagatorOptions tilePropagatorOptions = new TilePropagatorOptions();
            tilePropagatorOptions.RandomDouble = random.NextDouble;
            tilePropagatorOptions.BackTrackDepth = 250;

            //var borderConstraint = new BorderConstraint();
            // borderConstraint.Tiles = MapTile.Name.Water;

            this.propagator = new TilePropagator(tileModel: model, topology: topology, options: tilePropagatorOptions);
            this.propagatorOutput = null;

            this.ShowProgressBar();
        }

        public void ProcessNextGeneratorStep(bool generateOutputForThisStep, int processCount = 1)
        {
            SonOfRobinGame.game.IsFixedTimeStep = false;

            for (int i = 0; i < processCount; i++)
            {
                Resolution resolution = this.propagator.Step();

                switch (resolution)
                {
                    case Resolution.Decided:
                        this.FinishProcessing();
                        return;

                    case Resolution.Undecided:
                        break;

                    case Resolution.Contradiction:
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported resolution - {resolution}.");
                }

            }

            if (generateOutputForThisStep) this.propagatorOutput = propagator.ToValueArray<TileData.Name>();

            this.ShowProgressBar();
        }

        private void FinishProcessing()
        {
            this.processTime = DateTime.Now - this.creationTime;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Tile map procesing time ({this.width}x{this.height}): {this.processTime:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

            SonOfRobinGame.progressBar.TurnOff();
            SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;

            this.propagatorOutput = this.propagator.ToValueArray<TileData.Name>();

            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    TileData tileData = TileData.GetTile(this.propagatorOutput.Get(x, y));
                    Tile tile = new Tile(x: x, y: y, tileData: tileData);

                    this.allTiles.Add(tile);
                    this.tileBoard[x, y] = tile;
                }
            }

            this.model = null;
            this.topology = null;
            this.propagator = null;

            this.ReadyToUse = true;
        }

        public Texture2D GetTileTexture(int x, int y)
        {
            if (this.ReadyToUse)
            {
                return this.tileBoard[x, y].tileData.texture;
            }
            else
            {
                if (this.propagatorOutput == null) return null;

                TileData mapTile = TileData.GetTile(this.propagatorOutput.Get(x, y));
                return mapTile.texture;
            }
        }


        private void ShowProgressBar()
        {
            if (!this.showProgressBar) return;

            // int completeAmount = this.allElementsCount - this.elementsToCollapse.Count; // TODO find how to calculate this
            int completeAmount = 1;

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
            catch (OverflowException)
            { return TimeSpan.FromMinutes(30); }
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
