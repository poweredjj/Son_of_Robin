using System;
using System.Collections.Generic;
using System.Text;

namespace SonOfRobin
{
    class PlannedDestruction : PlannedEvent
    {
        public PlannedDestruction(World world, int delay, BoardPiece boardPiece) :
            base(world: world, delay: delay, boardPiece: boardPiece)
        { }

        public PlannedDestruction(World world, Dictionary<string, Object> eventData, Dictionary<string, BoardPiece> piecesByOldId) :
            base(world: world, eventData: eventData, piecesByOldId: piecesByOldId)
        {
            // deserialize
        }

        public override void Execute()
        { 
            this.boardPiece.Destroy();
        }


    }
}
