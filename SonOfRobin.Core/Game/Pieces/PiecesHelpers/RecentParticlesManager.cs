using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class RecentParticlesManager
    {
        private readonly Level level;
        private readonly HashSet<BoardPiece> pieceSet;
        private readonly List<ParticleEngine> distortionParticlesToDrawInThisFrame;
        public IEnumerable<BoardPiece> OffScreenPieces { get { return this.pieceSet.Where(piece => !level.world.camera.viewRect.Intersects(piece.sprite.GfxRect)); } }

        public RecentParticlesManager(Level level)
        {
            this.level = level;
            this.pieceSet = new HashSet<BoardPiece>();
            this.distortionParticlesToDrawInThisFrame = new List<ParticleEngine>();
        }

        public void AddPiece(BoardPiece piece)
        {
            this.pieceSet.Add(piece);
            if (piece.sprite.particleEngine.ActiveParticlesCountDistortion > 0) this.distortionParticlesToDrawInThisFrame.Add(piece.sprite.particleEngine);

            // SonOfRobinGame.messageLog.AddMessage(text: $"{SonOfRobinGame.CurrentUpdate} RecentParticles {this.pieceDict.Count - 1} -> {this.pieceDict.Count} ({piece.readableName})");
        }

        public void Update()
        {
            if (SonOfRobinGame.fps.FPS <= 25)
            {
                this.pieceSet.Clear();
                return;
            }

            Rectangle cameraRect = this.level.world.camera.viewRect;

            Vector2 cameraCenter = new(cameraRect.Center.X, cameraRect.Center.Y);
            int maxDistance = cameraRect.Width;

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
            // if (pieceCountCurrent != pieceCountPrevious) SonOfRobinGame.messageLog.AddMessage(text: $"{SonOfRobinGame.CurrentUpdate} RecentParticles {pieceCountPrevious} -> {pieceCountCurrent}");
        }

        public void DrawDistortion()
        {
            foreach (ParticleEngine particleEngine in this.distortionParticlesToDrawInThisFrame)
            {
                particleEngine.DrawDistortion();
            }

            this.distortionParticlesToDrawInThisFrame.Clear();
        }
    }
}