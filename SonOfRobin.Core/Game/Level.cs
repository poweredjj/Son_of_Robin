using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Level
    {
        public enum Type : byte { Island, Cave }

        public readonly Type levelType;
        public readonly int seed;
        public readonly Random random;
        public readonly int width;
        public readonly int height;
        public readonly Rectangle levelRect;
        public int maxAnimalsPerName;
        public readonly PieceCreationData[] creationDataArrayRegular;
        public readonly PieceCreationData[] creationDataArrayTemporaryDecorations;
        public readonly List<Sprite> temporaryDecorationSprites;

        public Level(Type type, int seed, int width, int height)
        {
            this.levelType = type;
            this.seed = seed;
            this.random = new Random(seed);
            this.width = width;
            this.height = height;
            this.levelRect = new Rectangle(x: 0, y: 0, width: this.width, height: this.height);

            var creationDataList = PieceCreationData.CreateDataList(maxAnimalsPerName: this.maxAnimalsPerName, levelType: this.levelType);
            this.creationDataArrayRegular = creationDataList.Where(data => !data.temporaryDecoration).ToArray();
            this.creationDataArrayTemporaryDecorations = creationDataList.Where(data => data.temporaryDecoration).ToArray();

            foreach (PieceCreationData pieceCreationData in this.creationDataArrayTemporaryDecorations)
            {
                if (PieceInfo.GetInfo(pieceCreationData.name).serialize) throw new ArgumentException($"Serialized piece cannot be temporary - {pieceCreationData.name}.");
            }

            this.maxAnimalsPerName = Math.Min((int)((float)this.width * (float)this.height * 0.0000008), 1000);
            MessageLog.Add(debugMessage: true, text: $"maxAnimalsPerName {maxAnimalsPerName}");

            this.temporaryDecorationSprites = new List<Sprite>();
        }
    }
}