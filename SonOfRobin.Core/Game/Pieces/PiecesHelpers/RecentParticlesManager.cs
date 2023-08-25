using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class RecentParticlesManager
    {
        private readonly World world;
        private readonly Dictionary<string, BoardPiece> pieceDict;
        public IEnumerable<BoardPiece> OffScreenPieces { get { return this.pieceDict.Values.Where(piece => !world.camera.viewRect.Intersects(piece.sprite.GfxRect)); } }

        public RecentParticlesManager(World world)
        {
            this.world = world;
            this.pieceDict = new Dictionary<string, BoardPiece>();
        }

        public void AddPiece(BoardPiece piece)
        {
            if (!this.pieceDict.ContainsKey(piece.id) && piece.sprite.IsInCameraRect)
            {
                this.pieceDict[piece.id] = piece;

                // MessageLog.AddMessage(msgType: MsgType.User, message: $"{SonOfRobinGame.CurrentUpdate} RecentParticles {this.pieceDict.Count - 1} -> {this.pieceDict.Count} ({piece.readableName})");
            }
        }

        public void Update()
        {
            if (SonOfRobinGame.fps.FPS <= 25)
            {
                this.pieceDict.Clear();
                return;
            }

            Vector2 cameraCenter = new(this.world.camera.viewRect.Center.X, this.world.camera.viewRect.Center.Y);
            int maxDistance = this.world.camera.viewRect.Width;

            // int pieceCountPrevious = this.pieceDict.Count;

            var keysToRemove = new List<string>();

            foreach (BoardPiece piece in this.pieceDict.Values)
            {
                if (piece.sprite.particleEngine == null ||
                    !piece.sprite.particleEngine.HasAnyParticles ||
                    Vector2.Distance(piece.sprite.position, cameraCenter) > maxDistance)
                {
                    keysToRemove.Add(piece.id);
                }
            }

            foreach (string pieceID in keysToRemove)
            {
                this.pieceDict.Remove(pieceID);
            }

            // int pieceCountCurrent = this.pieceDict.Count;
            // if (pieceCountCurrent != pieceCountPrevious) MessageLog.AddMessage(msgType: MsgType.User, message: $"{SonOfRobinGame.CurrentUpdate} RecentParticles {pieceCountPrevious} -> {pieceCountCurrent}");
        }
    }
}