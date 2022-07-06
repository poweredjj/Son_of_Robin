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
        public readonly WaveCollapseElement[,] tileBoard;

        private readonly DateTime creationTime;
        private TimeSpan processTime;

        public readonly int allElementsCount;
        public bool ReadyToUse { get; private set; }

        private AdjacentModel model;
        private GridTopology topology;
        private TilePropagator propagator;


        public TileMap(int width, int height, int seed = -1, bool showProgressBar = false)
        {
            this.creationTime = DateTime.Now;
            this.ReadyToUse = false;

            this.width = width;
            this.height = height;
            this.showProgressBar = showProgressBar;
            this.allElementsCount = this.width * this.height;

            this.seed = seed == -1 ? new Random().Next(9999) : seed;
            this.random = new Random(this.seed);


            this.model = new AdjacentModel(directions: DirectionSet.Cartesian2d);
            TileData.SetAdjacency(model);

            this.topology = new GridTopology(width: width, height: height, periodic: false);

            TilePropagatorOptions tilePropagatorOptions = new TilePropagatorOptions();
            tilePropagatorOptions.RandomDouble = random.NextDouble;
            //var borderConstraint = new BorderConstraint();
            // borderConstraint.Tiles = MapTile.Name.Water;


            var propagator = new TilePropagator(tileModel: model, topology: topology, options: tilePropagatorOptions);

            var status = propagator.Run();
            if (status != Resolution.Decided)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: "Could not collapse whole map correctly.");
                return;
            }




            if (this.showProgressBar) this.ShowProgressBar();
        }

        public void ProcessNextGenerateStep()
        {
            this.propagator.Step();


            if (propagator.Status == Resolution.Decided) this.FinishProcessing();

        }

        private void FinishProcessing()
        {

            var output = propagator.ToValueArray<TileData.Name>();
            string resultText = "";
            var imageList = new List<Texture2D>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    TileData mapTile = TileData.GetTile(output.Get(x, y));

                    imageList.Add(mapTile.texture);
                    resultText += "|";
                }
                if (y < height - 1) resultText += "\n";
            }


            this.CalculateNeighboursForAllElements();


            this.model = null;
            this.topology = null;
            this.propagator = null;

            this.ReadyToUse = true;
        }

        private void CalculateNeighboursForAllElements()
        {
            foreach (Tile tile in this.allTiles)
            {
                Vector2 basePos = new Vector2(tile.x, tile.y);

                List<Vector2> destPosList = new List<Vector2> {
                    new Vector2(basePos.X - 1, basePos.Y),
                    new Vector2(basePos.X + 1, basePos.Y),
                    new Vector2(basePos.X, basePos.Y - 1),
                    new Vector2(basePos.X, basePos.Y + 1)};

                List<Tile> neighbours = new List<Tile>();

                foreach (Vector2 destPos in destPosList)
                {
                    if (destPos.X >= 0 && destPos.X < this.width && destPos.Y >= 0 && destPos.Y < this.height) neighbours.Add(this.tileBoard[(int)destPos.X, (int)destPos.Y]);
                }

                tile.SetNeighbours(neighbours);
            }
        }

        private void ShowProgressBar()
        {
            int completeAmount = this.allElementsCount - this.elementsToCollapse.Count;

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
