using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Level
    {
        public enum LevelType : byte { Island, Cave, OpenSea }

        public readonly Level parentLevel;
        public readonly int depth;
        public readonly LevelType levelType;
        public readonly World world;
        public readonly int seed;
        public readonly Random random;
        public readonly int width;
        public readonly int height;
        public readonly Rectangle levelRect;
        public int maxAnimalsPerName;
        public readonly SMTypesManager stateMachineTypesManager;
        public readonly PieceCreationData[] creationDataArrayRegular;
        public readonly PieceCreationData[] creationDataArrayTemporaryDecorations;
        public readonly List<Sprite> temporaryDecorationSprites;
        public List<PieceTemplate.Name> doNotCreatePiecesList;
        public readonly LevelEventManager levelEventManager;
        public readonly TrackingManager trackingManager;
        public readonly RecentParticlesManager recentParticlesManager;
        public readonly Dictionary<Color, BoardPiece> mapMarkerByColor;
        public readonly List<Vector2> playerLastSteps;
        public readonly Grid grid;
        public Dictionary<PieceTemplate.Name, int> pieceCountByName;
        public Dictionary<Type, int> pieceCountByClass;
        public Queue<Cell> plantCellsQueue;
        public Queue<Sprite> plantSpritesQueue;
        public readonly HashSet<BoardPiece> heatedPieces;
        public Queue<Sprite> nonPlantSpritesQueue;
        public Vector2 playerReturnPos;
        public readonly bool hasWater;
        public readonly bool hasWeather;
        public readonly bool plansWeather;

        public bool creationInProgress;

        public Level(LevelType type, World world, int seed, int width, int height, bool hasWater = false, bool hasWeather = false, bool plansWeather = false, int cellWidthOverride = 0, int cellHeightOverride = 0, Dictionary<string, object> gridSerializedData = null)
        {
            this.parentLevel = world.ActiveLevel;
            this.depth = this.parentLevel == null ? 0 : parentLevel.depth + 1;
            this.levelType = type;
            this.hasWater = hasWater;
            this.hasWeather = hasWeather;
            this.plansWeather = plansWeather;
            this.world = world;
            this.seed = seed;
            this.random = new Random(seed);
            this.width = width;
            this.height = height;
            this.levelRect = new Rectangle(x: 0, y: 0, width: this.width, height: this.height);

            this.maxAnimalsPerName = Math.Min((int)((float)this.width * (float)this.height * 0.0000008), 1000);
            MessageLog.Add(debugMessage: true, text: $"maxAnimalsPerName {maxAnimalsPerName}");

            var creationDataList = PieceCreationData.CreateDataList(maxAnimalsPerName: this.maxAnimalsPerName, level: this);
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
            this.recentParticlesManager = new RecentParticlesManager(level: this);
            this.stateMachineTypesManager = new SMTypesManager(this);
            this.mapMarkerByColor = new Dictionary<Color, BoardPiece>
            {
                { Color.Blue, null },
                { new Color(15, 128, 0), null },
                { Color.Red, null },
                { new Color(93, 6, 99), null },
                { new Color(97, 68, 15), null },
            };
            this.playerLastSteps = new List<Vector2>();
            this.playerReturnPos = Vector2.Zero;

            this.grid = gridSerializedData == null ?
            new Grid(level: this, resDivider: this.levelType == LevelType.Island ? this.world.resDivider : 14, cellWidth: cellWidthOverride, cellHeight: cellHeightOverride) :
            Grid.Deserialize(level: this, gridData: gridSerializedData, resDivider: this.world.resDivider);

            this.creationInProgress = true;
        }

        // Level class should be treated as a data container. All processing should be done within World() class (if possible).

        ~Level()
        {
            MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} {this.levelType} level seed {this.seed} {this.width}x{this.height} no longer referenced.", textColor: new Color(120, 255, 174));
            this.Destroy(); // to dispose textures
        }

        public void Destroy()
        {
            this.grid.Destroy();
        }
    }
}