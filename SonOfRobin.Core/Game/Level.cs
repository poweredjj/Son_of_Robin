using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Level
    {
        public enum LevelType : byte { Island, Cave }

        public readonly LevelType levelType;
        public readonly World world;
        public readonly int seed;
        public readonly Random random;
        public readonly int width;
        public readonly int height;
        public readonly Rectangle levelRect;
        public int maxAnimalsPerName;
        public readonly PieceCreationData[] creationDataArrayRegular;
        public readonly PieceCreationData[] creationDataArrayTemporaryDecorations;
        public readonly List<Sprite> temporaryDecorationSprites;
        public List<PieceTemplate.Name> doNotCreatePiecesList;
        public readonly LevelEventManager levelEventManager;
        public readonly TrackingManager trackingManager;
        public Grid Grid { get; private set; }
        public Dictionary<PieceTemplate.Name, int> pieceCountByName;
        public Dictionary<Type, int> pieceCountByClass;

        public Queue<Cell> plantCellsQueue;
        public Queue<Sprite> plantSpritesQueue;
        public readonly HashSet<BoardPiece> heatedPieces;
        public Queue<Sprite> nonPlantSpritesQueue;

        public Level(LevelType type, World world, int seed, int width, int height)
        {
            this.levelType = type;
            this.world = world;
            this.seed = seed;
            this.random = new Random(seed);
            this.width = width;
            this.height = height;
            this.levelRect = new Rectangle(x: 0, y: 0, width: this.width, height: this.height);

            this.maxAnimalsPerName = Math.Min((int)((float)this.width * (float)this.height * 0.0000008), 1000);
            MessageLog.Add(debugMessage: true, text: $"maxAnimalsPerName {maxAnimalsPerName}");

            var creationDataList = PieceCreationData.CreateDataList(maxAnimalsPerName: this.maxAnimalsPerName, levelType: this.levelType);
            this.creationDataArrayRegular = creationDataList.Where(data => !data.temporaryDecoration).ToArray();
            this.creationDataArrayTemporaryDecorations = creationDataList.Where(data => data.temporaryDecoration).ToArray();
            this.doNotCreatePiecesList = new List<PieceTemplate.Name> { };

            foreach (PieceCreationData pieceCreationData in this.creationDataArrayTemporaryDecorations)
            {
                if (PieceInfo.GetInfo(pieceCreationData.name).serialize) throw new ArgumentException($"Serialized piece cannot be temporary - {pieceCreationData.name}.");
            }

            this.temporaryDecorationSprites = new List<Sprite>();

            this.pieceCountByName = new Dictionary<PieceTemplate.Name, int>();
            foreach (PieceTemplate.Name templateName in PieceTemplate.allNames) this.pieceCountByName[templateName] = 0;
            this.pieceCountByClass = new Dictionary<Type, int> { };

            this.plantSpritesQueue = new Queue<Sprite>();
            this.nonPlantSpritesQueue = new Queue<Sprite>();
            this.heatedPieces = new HashSet<BoardPiece>();
            this.plantCellsQueue = new Queue<Cell>();
            this.levelEventManager = new LevelEventManager(this);
            this.trackingManager = new TrackingManager(this);
        }

        public void Destroy()
        {
            this.Grid.Destroy();
        }

        public void AssignGrid(Grid grid)
        {
            if (this.Grid != null) throw new ArgumentException($"Grid has already been assigned to level {this.levelType}.");
            this.Grid = grid;
        }

        public void Update()
        {
            this.levelEventManager.ProcessQueue();
            this.trackingManager.ProcessQueue();
        }
    }
}