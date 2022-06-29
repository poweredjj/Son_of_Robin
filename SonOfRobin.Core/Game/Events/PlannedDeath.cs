using System;
using System.Collections.Generic;
using System.Text;

namespace SonOfRobin
{
    class PlannedDeath : PlannedEvent
    {
        public PlannedDeath(World world, int delay, BoardPiece boardPiece) :
            base(world: world, delay: delay, boardPiece: boardPiece)
        { }
        public PlannedDeath(World world, Dictionary<string, Object> eventData, Dictionary<string, BoardPiece> piecesByOldId) :
            base(world: world, eventData: eventData, piecesByOldId: piecesByOldId)
        {
            // deserialize
        }

        public override void Execute()
        {
            if (this.boardPiece.alive) this.boardPiece.Kill();
        }

    }
}
