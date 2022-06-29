using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class WorldEvent
    {
        public enum EventName { Birth, Death, Destruction, TurnOffWorkshop, FinishCooking, RestorePieceCreation, FadeOutSprite, RestoreHint, RemoveBuff }

        public readonly World world;
        public readonly BoardPiece boardPiece;
        public readonly int startUpdateNo;
        public readonly EventName eventName;
        public readonly Object eventHelper;

        public WorldEvent(EventName eventName, World world, int delay, BoardPiece boardPiece, Object eventHelper = null)
        {
            this.eventName = eventName;
            this.eventHelper = eventHelper;
            this.world = world;
            this.boardPiece = boardPiece;
            delay = Math.Max(delay, 0);
            this.startUpdateNo = this.world.currentUpdate + delay;

            this.AddToQueue();

            if (this.eventName == EventName.Destruction) new WorldEvent(eventName: EventName.FadeOutSprite, delay: delay - OpacityFade.defaultDuration, world: world, boardPiece: boardPiece);
        }

        public Dictionary<string, Object> Serialize()
        {
            var eventData = new Dictionary<string, Object> {
                {"eventName", this.eventName},
                {"pieceOldId", this.boardPiece?.id},
                {"startUpdateNo", this.startUpdateNo},
                {"eventHelper", this.eventHelper},
            };

            return eventData;
        }

        public static void Deserialize(World world, Dictionary<string, Object> eventData, Dictionary<string, BoardPiece> piecesByOldId)
        {
            // for events that target a piece, that was already destroyed (and will not be present in saved data)
            EventName eventName = (EventName)eventData["eventName"];

            BoardPiece boardPiece;

            if (eventName == EventName.RestorePieceCreation || eventName == EventName.RestoreHint)
            { boardPiece = null; }
            else
            {
                if (!piecesByOldId.ContainsKey((string)eventData["pieceOldId"])) return;
                boardPiece = piecesByOldId[(string)eventData["pieceOldId"]];
            }

            int startUpdateNo = (int)eventData["startUpdateNo"];
            int delay = Math.Max(startUpdateNo - world.currentUpdate, 0);
            Object eventHelper = eventData["eventHelper"];

            new WorldEvent(eventName: eventName, world: world, delay: delay, boardPiece: boardPiece, eventHelper: eventHelper);
        }

        private void AddToQueue()
        {
            if (!this.world.eventQueue.ContainsKey(this.startUpdateNo)) this.world.eventQueue[this.startUpdateNo] = new List<WorldEvent>();

            this.world.eventQueue[this.startUpdateNo].Add(this);
        }

        public static void RemovePieceFromQueue(BoardPiece pieceToRemove, World world)
        {
            List<WorldEvent> eventlist;

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
                foreach (WorldEvent currentEvent in world.eventQueue[frameNo])
                { currentEvent.Execute(); }

                world.eventQueue.Remove(frameNo);
            }
        }

        private void Execute()
        {
            switch (this.eventName)
            {
                case EventName.Birth:
                    Animal motherAnimal = (Animal)this.boardPiece;
                    if (!motherAnimal.alive) return;

                    motherAnimal.activeState = BoardPiece.State.AnimalGiveBirth;
                    motherAnimal.aiData.Reset();
                    return;

                case EventName.Death:
                    if (this.boardPiece.alive) this.boardPiece.Kill();
                    return;

                case EventName.Destruction:
                    this.boardPiece.Destroy();
                    return;

                case EventName.TurnOffWorkshop:
                    Workshop workshop = (Workshop)this.boardPiece;
                    workshop.TurnOff();
                    return;

                case EventName.FinishCooking:
                    Cooker cooker = (Cooker)this.boardPiece;
                    cooker.TurnOff();
                    cooker.boardTask = Scheduler.TaskName.OpenContainer;
                    return;


                case EventName.FadeOutSprite:
                    this.boardPiece.sprite.opacityFade = new OpacityFade(sprite: this.boardPiece.sprite, destOpacity: 0f);
                    return;

                case EventName.RemoveBuff:
                    int buffId = (int)this.eventHelper;
                    this.boardPiece.buffEngine.RemoveBuff(buffId);

                    return;

                case EventName.RestorePieceCreation:
                    var pieceName = (PieceTemplate.Name)this.eventHelper;
                    this.world.doNotCreatePiecesList.Remove(pieceName);

                    MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"'{pieceName}' creation restored.");

                    return;

                case EventName.RestoreHint:
                    var hintType = (HintEngine.Type)this.eventHelper;
                    this.world.hintEngine.EnableType(hintType);

                    MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Hint '{hintType}' restored.");

                    return;

                default:
                    throw new DivideByZeroException($"Unsupported eventName - {eventName}.");
            }
        }

    }
}
