using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SonOfRobin
{
    public class PlannedEvent
    {
        public readonly World world;
        public readonly BoardPiece boardPiece;
        public readonly PieceTemplate.Name name;
        public readonly int startUpdateNo;

        public PlannedEvent(World world, int delay, BoardPiece boardPiece)
        {
            this.world = world;
            this.boardPiece = boardPiece;
            this.name = boardPiece.name;
            this.startUpdateNo = this.world.currentUpdate + delay;

            this.AddToQueue();
        }

        public PlannedEvent(World world, Dictionary<string, Object> eventData, Dictionary<string, BoardPiece> piecesByOldId)
        // deserialize - step 2: deserialize data
        {
            this.world = world;
            this.boardPiece = piecesByOldId[(string)eventData["piece_old_id"]];
            this.name = boardPiece.name;
            this.startUpdateNo = (int)eventData["startUpdateNo"];

            this.AddToQueue();
        }

        public static void Deserialize(World world, Dictionary<string, Object> eventData, Dictionary<string, BoardPiece> piecesByOldId)
        // deserialize - step 1: choosing a proper class
        {
            // for events that target a piece, that was already destroyed (and will not be present in saved data)
            if (!piecesByOldId.ContainsKey((string)eventData["piece_old_id"])) return;

            switch (eventData["class_name"])
            {
                case "PlannedDeath":
                    new PlannedDeath(world: world, eventData: eventData, piecesByOldId: piecesByOldId);
                    break;

                case "PlannedDestruction":
                    new PlannedDestruction(world: world, eventData: eventData, piecesByOldId: piecesByOldId);
                    break;

                case "PlannedBirth":
                    new PlannedBirth(world: world, eventData: eventData, piecesByOldId: piecesByOldId);
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported event class name - {eventData["class_name"]}.");
            }
        }

        public Dictionary<string, Object> Serialize()
        {
            var eventData = new Dictionary<string, Object> {
                {"class_name", (string)this.GetType().Name},
                {"piece_old_id", this.boardPiece.id},
                {"startUpdateNo", this.startUpdateNo},
            };

            return eventData;
        }

        private void AddToQueue()
        {
            if (!this.world.eventQueue.ContainsKey(this.startUpdateNo))
            { this.world.eventQueue[this.startUpdateNo] = new List<PlannedEvent>(); }

            this.world.eventQueue[this.startUpdateNo].Add(this);
        }

        public static void RemovePieceFromQueue(BoardPiece pieceToRemove, World world)
        {
            List<PlannedEvent> eventlist;

            foreach (var frame in world.eventQueue.Keys.ToList())
            {
                eventlist = world.eventQueue[frame].Where(plannedEvent => plannedEvent.boardPiece != pieceToRemove).ToList();
                world.eventQueue[frame] = eventlist;
            }
        }

        public static void ProcessQueue(World world)
        {
            var framesToProcess = world.eventQueue.Keys.Where(frameNo => world.currentUpdate >= frameNo).ToList();
            if (framesToProcess.Count == 0) return;

            foreach (int frameNo in framesToProcess)
            {
                foreach (PlannedEvent currentEvent in world.eventQueue[frameNo])
                { currentEvent.Execute(); }

                world.eventQueue.Remove(frameNo);
            }
        }

        public virtual void Execute()
        { throw new DivideByZeroException("PlannedEvent constructor should not be invoked directly."); }

    }
}
