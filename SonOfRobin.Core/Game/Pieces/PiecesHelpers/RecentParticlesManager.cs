using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class RecentParticlesManager
    {
        private readonly World world;
        private readonly HashSet<BoardPiece> pieceSet;
        public IEnumerable<BoardPiece> OffScreenPieces { get { return this.pieceSet.Where(piece => !world.camera.viewRect.Intersects(piece.sprite.GfxRect)); } }

        public RecentParticlesManager(World world)
        {
            this.world = world;
            this.pieceSet = new HashSet<BoardPiece>();
        }

        public void AddPiece(BoardPiece piece)
        {
            this.pieceSet.Add(piece);
            // MessageLog.AddMessage(msgType: MsgType.User, message: $"{SonOfRobinGame.CurrentUpdate} RecentParticles {this.pieceDict.Count - 1} -> {this.pieceDict.Count} ({piece.readableName})");
        }

        public void Update()
        {
            if (SonOfRobinGame.fps.FPS <= 25)
            {
                this.pieceSet.Clear();
                return;
            }

            Vector2 cameraCenter = new(this.world.camera.viewRect.Center.X, this.world.camera.viewRect.Center.Y);
            int maxDistance = this.world.camera.viewRect.Width;

            // int pieceCountPrevious = this.pieceSet.Count;

            var piecesToRemove = new List<BoardPiece>();

            foreach (BoardPiece piece in this.pieceSet)
            {
                if (piece.sprite.particleEngine == null ||
                    !piece.sprite.particleEngine.HasAnyParticles ||
                    Vector2.Distance(piece.sprite.position, cameraCenter) > maxDistance)
                {
                    piecesToRemove.Add(piece);
                }
            }

            foreach (BoardPiece pieceToRemove in piecesToRemove)
            {
                this.pieceSet.Remove(pieceToRemove);
            }

            // int pieceCountCurrent = this.pieceSet.Count;
            // if (pieceCountCurrent != pieceCountPrevious) MessageLog.AddMessage(msgType: MsgType.User, message: $"{SonOfRobinGame.CurrentUpdate} RecentParticles {pieceCountPrevious} -> {pieceCountCurrent}");
        }
    }
}