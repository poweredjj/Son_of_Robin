using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class WorldEventManager
    {
        public readonly World world;
        private readonly Dictionary<int, Queue<WorldEvent>> eventQueue;

        public WorldEventManager(World world)
        {
            this.world = world;
            this.eventQueue = new Dictionary<int, Queue<WorldEvent>>();
        }

        public void AddToQueue(WorldEvent worldEvent, bool addFadeOut = true)
        {
            if (!this.eventQueue.ContainsKey(worldEvent.startUpdateNo)) this.eventQueue[worldEvent.startUpdateNo] = new Queue<WorldEvent>();
            this.eventQueue[worldEvent.startUpdateNo].Enqueue(worldEvent);

            if (worldEvent.eventName == WorldEvent.EventName.Destruction && addFadeOut)
            {
                if (worldEvent.boardPiece.GetType() == typeof(Animal))
                {
                    // dead animal should explode instead of fading out
                    new WorldEvent(eventName: WorldEvent.EventName.DropDebris, delay: worldEvent.delay - 1, world: world, boardPiece: worldEvent.boardPiece);
                }
                else
                {
                    int fadeDuration = OpacityFade.defaultDuration;
                    new WorldEvent(eventName: WorldEvent.EventName.FadeOutSprite, delay: worldEvent.delay - fadeDuration, world: world, boardPiece: worldEvent.boardPiece, eventHelper: fadeDuration);
                }
            }
        }

        public void RemovePieceFromQueue(BoardPiece pieceToRemove)
        {
            // Pieces removed from the board should not be removed from the queue (CPU intensive) - will be ignored when run.

            foreach (int frame in this.eventQueue.Keys.ToList())
            {
                this.eventQueue[frame] = new Queue<WorldEvent>(this.eventQueue[frame].Where(plannedEvent => plannedEvent.boardPiece != pieceToRemove));
            }
        }

        public void ProcessQueue()
        {
            var framesToProcess = this.eventQueue.Keys
                .Where(frameNo => world.CurrentUpdate >= frameNo)
                .OrderBy(frameNo => frameNo)
                .ToList();

            if (framesToProcess.Count == 0) return;

            foreach (int frameNo in framesToProcess)
            {
                foreach (WorldEvent currentEvent in this.eventQueue[frameNo])
                { currentEvent.Execute(this.world); }

                this.eventQueue.Remove(frameNo);
            }
        }

        public List<Object> Serialize()
        {
            var eventData = new List<Object> { };
            foreach (var eventList in this.eventQueue.Values)
            {
                foreach (var plannedEvent in eventList)
                {
                    if (plannedEvent.CanBeSerialized && (plannedEvent.boardPiece == null || plannedEvent.boardPiece.exists))
                    {
                        eventData.Add(plannedEvent.Serialize());
                    }
                }
            }

            return eventData;
        }

        public void Deserialize(List<Object> eventDataList)
        {
            foreach (Dictionary<string, Object> eventData in eventDataList)
            {
                WorldEvent worldEvent = WorldEvent.Deserialize(world: this.world, eventData: eventData);
                if (worldEvent != null) this.AddToQueue(worldEvent: worldEvent, addFadeOut: false);
            }
        }
    }

    public class WorldEvent
    {
        public enum EventName : byte
        {
            Birth = 0,
            Death = 1,
            Destruction = 2,
            TurnOffWorkshop = 3,
            FinishCooking = 4,
            RestorePieceCreation = 5,
            FadeOutSprite = 6,
            RestoreHint = 7,
            RemoveBuff = 8,
            BurnOutLightSource = 9,
            RegenPoison = 10,
            ChangeActiveState = 11,
            FinishBuilding = 12,
            PlaySoundByName = 13,
            YieldDropDebris = 14,
            AnimalCallForHelp = 15,
            FinishBrewing = 16,
            StopBurning = 17,
            TurnOffHarvestingWorkshop = 18,
            DropDebris = 19,
            SwitchLightEngine = 20,
            TotemAffectWeather = 21,
            CheckForPieceHints = 22,
        }

        // some events can't be serialized properly (cannot serialize some eventHelpers - like BoardPiece), but can safely be ignored
        private readonly List<EventName> nonSerializedEvents = new() { EventName.AnimalCallForHelp, EventName.YieldDropDebris, EventName.CheckForPieceHints };

        public readonly BoardPiece boardPiece;
        public readonly int startUpdateNo;
        public readonly int delay;
        public readonly EventName eventName;
        public readonly Object eventHelper;

        public bool CanBeSerialized
        { get { return !nonSerializedEvents.Contains(this.eventName); } }

        public WorldEvent(EventName eventName, World world, int delay, BoardPiece boardPiece, Object eventHelper = null, bool addToQueue = true)
        {
            this.eventName = eventName;
            this.eventHelper = eventHelper;
            this.boardPiece = boardPiece;
            this.delay = Math.Max(delay, 1); // to prevent from modifying current frame queue
            this.startUpdateNo = world.CurrentUpdate + this.delay;

            if (addToQueue) world.worldEventManager.AddToQueue(worldEvent: this, addFadeOut: true);
        }

        public Dictionary<string, Object> Serialize()
        {
            var eventData = new Dictionary<string, Object> {
                { "eventName", this.eventName },
                { "piece_id", this.boardPiece?.id },
                { "startUpdateNo", this.startUpdateNo },
                { "eventHelper", this.eventHelper },
            };

            return eventData;
        }

        public static WorldEvent Deserialize(World world, Dictionary<string, Object> eventData)
        {
            // for events that target a piece, that was already destroyed (and will not be present in saved data)
            EventName eventName = (EventName)(Int64)eventData["eventName"];

            var eventsWithoutPieces = new HashSet<EventName> { EventName.RestorePieceCreation, EventName.RestoreHint, EventName.FinishBuilding };

            BoardPiece boardPiece;
            if (eventsWithoutPieces.Contains(eventName)) boardPiece = null;
            else
            {
                int pieceID = (int)(Int64)eventData["piece_id"];

                if (!world.piecesByIDForDeserialization.ContainsKey(pieceID))
                {
                    SonOfRobinGame.MessageLog.Add(debugMessage: true, text: $"WorldEvent {eventName} - cannot find boardPiece id {pieceID}.", textColor: Color.Orange);
                    return null;
                }
                boardPiece = world.piecesByIDForDeserialization[pieceID];
            }

            int startUpdateNo = (int)(Int64)eventData["startUpdateNo"];
            int delay = Math.Max(startUpdateNo - world.CurrentUpdate, 0);
            Object eventHelper = eventData["eventHelper"];

            if (eventHelper != null)
            {
                try
                { eventHelper = Helpers.CastObjectToInt(eventHelper); } // serialization makes every int stored as int64 (long)
                catch (Exception)
                { }
            }

            return new WorldEvent(eventName: eventName, world: world, delay: delay, boardPiece: boardPiece, eventHelper: eventHelper, addToQueue: false);
        }

        public void Execute(World world)
        {
            if (this.boardPiece != null && !this.boardPiece.exists) return;

            switch (this.eventName)
            {
                case EventName.Birth:  // should only be used for pieces, that are processed every frame
                    {
                        Animal motherAnimal = (Animal)this.boardPiece;
                        if (!motherAnimal.alive) return;

                        motherAnimal.activeState = BoardPiece.State.AnimalGiveBirth;
                        motherAnimal.aiData.Reset();
                        return;
                    }

                case EventName.Death: // should only be used for pieces, that are processed every frame (none right now)
                    {
                        if (this.boardPiece.alive) this.boardPiece.Kill();
                        return;
                    }

                case EventName.Destruction:
                    {
                        if (!this.boardPiece.sprite.IsOnBoard) return;

                        if (this.boardPiece.GetType() == typeof(Animal) && this.boardPiece.sprite.IsOnBoard && this.boardPiece.sprite.IsInCameraRect)
                        {
                            PieceTemplate.CreateAndPlaceOnBoard(world: this.boardPiece.world, position: this.boardPiece.sprite.position, templateName: PieceTemplate.Name.BloodSplatter);

                            this.boardPiece.pieceInfo.Yield.DropDebris(piece: this.boardPiece);
                        }
                        this.boardPiece.Destroy();
                        return;
                    }

                case EventName.TurnOffWorkshop:
                    {
                        Workshop workshop = (Workshop)this.boardPiece;
                        workshop.TurnOff();
                        return;
                    }

                case EventName.TurnOffHarvestingWorkshop:
                    {
                        MeatHarvestingWorkshop harvestingWorkshop = (MeatHarvestingWorkshop)this.boardPiece;
                        harvestingWorkshop.TurnOff();
                        return;
                    }

                case EventName.DropDebris:
                    {
                        this.boardPiece.pieceInfo?.Yield.DropDebris(piece: this.boardPiece, particlesToEmit: 50);
                        return;
                    }

                case EventName.FinishCooking:
                    {
                        Cooker cooker = (Cooker)this.boardPiece;
                        cooker.TurnOff();
                        return;
                    }

                case EventName.FinishBrewing:
                    {
                        AlchemyLab alchemyLab = (AlchemyLab)this.boardPiece;
                        alchemyLab.TurnOff();
                        return;
                    }

                case EventName.StopBurning:
                    {
                        // SonOfRobinGame.messageLog.AddMessage(text: $"{SonOfRobinGame.CurrentUpdate} {this.boardPiece.readableName} cooling.", color: Color.LightCyan);
                        this.boardPiece.HeatLevel = Math.Min(this.boardPiece.HeatLevel, BoardPiece.minBurnVal - 0.1f);

                        return;
                    }

                case EventName.FadeOutSprite:
                    {
                        if (!this.boardPiece.sprite.IsInCameraRect) return;

                        int fadeDuration = Helpers.CastObjectToInt(this.eventHelper);
                        new OpacityFade(sprite: this.boardPiece.sprite, destOpacity: 0f, duration: fadeDuration);
                        return;
                    }

                case EventName.RemoveBuff:
                    {
                        int buffID = Helpers.CastObjectToInt(this.eventHelper);
                        this.boardPiece.buffEngine.RemoveBuff(buffID: buffID, checkIfHasThisBuff: false);

                        return;
                    }

                case EventName.RestorePieceCreation:
                    {
                        var pieceName = (PieceTemplate.Name)Helpers.CastObjectToUshort(this.eventHelper);
                        world.doNotCreatePiecesList.Remove(pieceName);

                        SonOfRobinGame.MessageLog.Add(debugMessage: true, text: $"'{pieceName}' creation restored.");

                        return;
                    }

                case EventName.RestoreHint:
                    {
                        var hintType = (HintEngine.Type)Helpers.CastObjectToByte(this.eventHelper);
                        world.HintEngine.Enable(hintType);

                        SonOfRobinGame.MessageLog.Add(debugMessage: true, text: $"Hint '{hintType}' restored.");

                        return;
                    }

                case EventName.BurnOutLightSource:
                    {
                        // example eventHelper for this task
                        // var damageData = new Dictionary<string, Object> { { "delay", 60 * 3 }, { "damage", 3 } };

                        PortableLight portableLight = (PortableLight)this.boardPiece;

                        // breaking damage loop

                        if (world.Player == null ||
                            !world.Player.alive ||
                            !world.Player.exists ||
                            world.Player.sprite.IsInWater ||
                            world.Player.sleepMode != Player.SleepMode.Awake ||
                            !portableLight.IsOnPlayersToolbar ||
                            !portableLight.IsOn ||
                            (!portableLight.canBeUsedDuringRain && world.weather.IsRaining))
                        {
                            portableLight.IsOn = false;
                            return;
                        }

                        // reading damage data

                        var damageData = (Dictionary<string, Object>)this.eventHelper;

                        int delay = Helpers.CastObjectToInt(damageData["delay"]);
                        int damage = Helpers.CastObjectToInt(damageData["damage"]);

                        // inflicting damage

                        // SonOfRobinGame.messageLog.AddMessage(text: $"{this.boardPiece.readableName} HP {this.boardPiece.hitPoints} - {damage}"); // for testing

                        this.boardPiece.HitPoints = Math.Max(this.boardPiece.HitPoints - damage, 0);
                        if (this.boardPiece.HitPoints <= 0)
                        {
                            world.HintEngine.ShowGeneralHint(type: HintEngine.Type.BurntOutTorch, ignoreDelay: true, text: portableLight.readableName, texture: portableLight.sprite.AnimFrame.texture);
                            SonOfRobinGame.MessageLog.Add(text: $"{Helpers.FirstCharToUpperCase(this.boardPiece.readableName)} has burnt out.", texture: this.boardPiece.sprite.AnimFrame.texture, bgColor: new Color(105, 3, 18), textColor: Color.White);

                            portableLight.IsOn = false;

                            if (portableLight.convertsToWhenUsedUp != PieceTemplate.Name.Empty)
                            {
                                StorageSlot slot = portableLight.world.Player.ToolStorage.FindSlotContainingThisPiece(portableLight);

                                BoardPiece emptyContainter = PieceTemplate.CreatePiece(templateName: portableLight.convertsToWhenUsedUp, world: world);
                                slot.DestroyPieceAndReplaceWithAnother(emptyContainter);
                            }
                            else world.Player.EquipStorage.DestroyBrokenPieces();
                        }

                        // setting next loop event

                        new WorldEvent(eventName: EventName.BurnOutLightSource, world: world, delay: delay, boardPiece: this.boardPiece, eventHelper: this.eventHelper);
                        return;
                    }

                case EventName.SwitchLightEngine:
                    {
                        bool activate = (bool)this.eventHelper;

                        if (activate) this.boardPiece.sprite.lightEngine.Activate();
                        else this.boardPiece.sprite.lightEngine.Deactivate();

                        return;
                    }

                case EventName.TotemAffectWeather:
                    {
                        float goodOfferingMass = Helpers.CastObjectToFloat(this.eventHelper);
                        Totem totem = (Totem)this.boardPiece;
                        totem.AffectWeather(goodOfferingMass);

                        return;
                    }

                case EventName.RegenPoison:
                    {
                        // example eventHelper for this task
                        // var regenPoisonData = new Dictionary<string, Object> { { "buffID", buff.id }, { "charges", buff.autoRemoveDelay / delay }, { "delay", delay }, { "hpChange", buff.value }, { "canKill", buff.canKill }};

                        // reading regen / poison data

                        var regenPoisonData = (Dictionary<string, Object>)this.eventHelper;

                        int buffID = Helpers.CastObjectToInt(regenPoisonData["buffID"]);
                        int delay = Helpers.CastObjectToInt(regenPoisonData["delay"]);
                        int charges = Helpers.CastObjectToInt(regenPoisonData["charges"]);
                        int hpChange = Helpers.CastObjectToInt(regenPoisonData["hpChange"]);

                        bool canKill = (bool)regenPoisonData["canKill"];

                        // breaking the loop

                        if (charges <= 0 || !this.boardPiece.alive || !this.boardPiece.exists || !this.boardPiece.buffEngine.HasBuff(buffID))
                        {
                            // no need to remove the buff - autoRemoveDelay will remove it
                            return;
                        }

                        // flashing the screen red if reducing player's hit points

                        if (hpChange < 0 && this.boardPiece.GetType() == typeof(Player))
                        {
                            SolidColor redOverlay = new SolidColor(color: Color.Red * 0.8f, viewOpacity: 0.0f);
                            redOverlay.transManager.AddTransition(new Transition(transManager: redOverlay.transManager, outTrans: true, duration: 16, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.5f, endRemoveScene: true));
                            world.solidColorManager.Add(redOverlay);
                            this.boardPiece.activeSoundPack.Play(PieceSoundPackTemplate.Action.IsHit);
                        }

                        // changing target hit points

                        int minValue = canKill ? 0 : 1;

                        this.boardPiece.HitPoints += hpChange;
                        this.boardPiece.HitPoints = Math.Max(this.boardPiece.HitPoints, minValue);
                        if (this.boardPiece.HitPoints == 0)
                        {
                            this.boardPiece.Kill();
                            return;
                        }

                        // setting next loop event

                        charges--;
                        regenPoisonData["charges"] = charges; // updating charges counter (the rest should stay the same)

                        new WorldEvent(eventName: EventName.RegenPoison, world: world, delay: delay, boardPiece: this.boardPiece, eventHelper: regenPoisonData);

                        return;
                    }

                case EventName.ChangeActiveState:
                    {
                        // example eventHelper for this task
                        // var changeStateData = new Dictionary<string, Object> { { "piece", piece }, { "state", state } };

                        var changeStateData = (Dictionary<string, Object>)this.eventHelper;

                        BoardPiece piece = (BoardPiece)changeStateData["piece"];
                        BoardPiece.State state = (BoardPiece.State)changeStateData["state"];

                        piece.activeState = state;

                        return;
                    }

                case EventName.FinishBuilding:
                    {
                        world.ExitBuildMode(restoreCraftMenu: false, showCraftMessages: true);
                        return;
                    }

                case EventName.PlaySoundByName:
                    {
                        Sound.QuickPlay((SoundData.Name)Helpers.CastObjectToByte(this.eventHelper));
                        return;
                    }

                case EventName.YieldDropDebris:
                    {
                        // example eventHelper for this task
                        // var eventHelper = new Dictionary<string, Object> { { "piece", piece }, { "yield", yield } };

                        var eventHelperDict = (Dictionary<string, Object>)this.eventHelper;
                        Yield yield = (Yield)eventHelperDict["yield"];
                        BoardPiece piece = (BoardPiece)eventHelperDict["piece"];

                        yield.DropDebris(piece: piece);

                        return;
                    }

                case EventName.AnimalCallForHelp:
                    {
                        Animal animal = (Animal)this.boardPiece;
                        BoardPiece target = (BoardPiece)this.eventHelper;

                        if (!animal.alive || Vector2.Distance(animal.sprite.position, target.sprite.position) > animal.pieceInfo.animalSightRange) return;

                        animal.target = target;
                        animal.aiData.Reset();
                        animal.activeState = BoardPiece.State.AnimalCallForHelp;

                        return;
                    }

                case EventName.CheckForPieceHints:
                    {
                        // WorldEvent variant - resistant to Scheduler.ClearQueue()

                        // eventHelper for this event should be identical to Scheduler.TaskName.CheckForPieceHints

                        if (world.CineMode ||
                            !world.inputActive ||
                            world.Player.activeState != BoardPiece.State.PlayerControlledWalking)
                        {
                            new WorldEvent(eventName: this.eventName, world: world, delay: 60 * 2, boardPiece: null, eventHelper: this.eventHelper);
                            return;
                        }

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecutePieceHintCheckNow, delay: 0, executeHelper: this.eventHelper);

                        return;
                    }

                default:
                    throw new ArgumentException($"Unsupported eventName - {eventName}.");
            }
        }
    }
}