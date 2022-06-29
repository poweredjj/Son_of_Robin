using System;
using System.Collections.Generic;
using System.Text;

namespace SonOfRobin
{
    class PlannedBirth : PlannedEvent
    {
        public PlannedBirth(World world, int delay, BoardPiece boardPiece) :
            base(world: world, delay: delay, boardPiece: boardPiece)
        { }

        public PlannedBirth(World world, Dictionary<string, Object> eventData, Dictionary<string, BoardPiece> piecesByOldId) :
            base(world: world, eventData: eventData, piecesByOldId: piecesByOldId)
        {
            // deserialize
        }

        public override void Execute()
        {
            Animal motherAnimal = (Animal)this.boardPiece;

            if (!motherAnimal.alive) return;

            motherAnimal.activeState = BoardPiece.State.AnimalGiveBirth;
            motherAnimal.aiData.Reset();
        }

    }
}
