﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class WorldEvent
    {
        public enum EventName { Birth, Death, Destruction, TurnOffWorkshop, FinishCooking, RestorePieceCreation, FadeOutSprite, RestoreHint, RemoveBuff, BurnOutLightSource, RegenPoison }

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

            if (this.eventName == EventName.Destruction)
            {
                int fadeDuration = this.boardPiece.GetType() == typeof(Animal) ? delay - 1 : OpacityFade.defaultDuration;

                new WorldEvent(eventName: EventName.FadeOutSprite, delay: delay - fadeDuration, world: world, boardPiece: boardPiece, eventHelper: fadeDuration);
            }
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
                case EventName.Birth:  // should only be used for pieces, that are processed every frame
                    {
                        Animal motherAnimal = (Animal)this.boardPiece;
                        if (!motherAnimal.alive) return;

                        motherAnimal.activeState = BoardPiece.State.AnimalGiveBirth;
                        motherAnimal.aiData.Reset(motherAnimal);
                        return;
                    }

                case EventName.Death: // should only be used for pieces, that are processed every frame
                    {
                        if (this.boardPiece.alive) this.boardPiece.Kill();
                        return;
                    }

                case EventName.Destruction:
                    {
                        this.boardPiece.Destroy();
                        return;
                    }

                case EventName.TurnOffWorkshop:
                    {
                        Workshop workshop = (Workshop)this.boardPiece;
                        workshop.TurnOff();
                        return;
                    }

                case EventName.FinishCooking:
                    {
                        Cooker cooker = (Cooker)this.boardPiece;
                        cooker.TurnOff();
                        cooker.boardTask = Scheduler.TaskName.OpenContainer;
                        return;
                    }

                case EventName.FadeOutSprite:
                    {
                        int fadeDuration = (int)this.eventHelper;
                        this.boardPiece.sprite.opacityFade = new OpacityFade(sprite: this.boardPiece.sprite, destOpacity: 0f, duration: fadeDuration);
                        return;
                    }

                case EventName.RemoveBuff:
                    {
                        int buffID = (int)this.eventHelper;
                        this.boardPiece.buffEngine.RemoveBuff(buffID);

                        return;
                    }

                case EventName.RestorePieceCreation:
                    {
                        var pieceName = (PieceTemplate.Name)this.eventHelper;
                        this.world.doNotCreatePiecesList.Remove(pieceName);

                        MessageLog.AddMessage(msgType: MsgType.Debug, message: $"'{pieceName}' creation restored.");

                        return;
                    }

                case EventName.RestoreHint:
                    {
                        var hintType = (HintEngine.Type)this.eventHelper;
                        this.world.hintEngine.Enable(hintType);

                        MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Hint '{hintType}' restored.");

                        return;
                    }

                case EventName.BurnOutLightSource:
                    {
                        // example eventHelper for this task
                        // var damageData = new Dictionary<string, Object> { { "delay", 60 * 3 }, { "damage", 3 } };

                        PortableLight portableLight = (PortableLight)this.boardPiece;

                        // breaking damage loop

                        if (this.world.player == null || !this.world.player.alive || !this.world.player.exists || this.world.player.sprite.IsInWater || !portableLight.IsOnPlayersToolbar || !portableLight.IsOn)
                        {
                            portableLight.IsOn = false;
                            return;
                        }

                        // reading damage data

                        var damageData = (Dictionary<string, Object>)this.eventHelper;

                        int delay = (int)damageData["delay"];
                        int damage = (int)damageData["damage"];

                        // inflicting damage

                        this.boardPiece.hitPoints = Math.Max(this.boardPiece.hitPoints - damage, 0);
                        if (this.boardPiece.hitPoints <= 0)
                        {
                            this.world.hintEngine.ShowGeneralHint(type: HintEngine.Type.BurntOutTorch, ignoreDelay: true, text: portableLight.readableName, texture: portableLight.sprite.frame.texture);
                            MessageLog.AddMessage(msgType: MsgType.User, message: $"{Helpers.FirstCharToUpperCase(this.boardPiece.readableName)} has burnt out.", color: Color.White);

                            portableLight.IsOn = false;
                            this.world.player.equipStorage.DestroyBrokenPieces();
                        }

                        // setting next loop event

                        new WorldEvent(eventName: EventName.BurnOutLightSource, world: this.world, delay: delay, boardPiece: this.boardPiece, eventHelper: this.eventHelper);
                        return;
                    }

                case EventName.RegenPoison:
                    {
                        // example eventHelper for this task
                        // var regenPoisonData = new Dictionary<string, Object> { { "buffID", buff.id }, { "charges", buff.autoRemoveDelay / delay }, { "delay", delay }, { "hpChange", buff.value }, { "canKill", buff.canKill }};

                        // reading regen / poison data

                        var regenPoisonData = (Dictionary<string, Object>)this.eventHelper;

                        int buffID = (int)regenPoisonData["buffID"];
                        int delay = (int)regenPoisonData["delay"];
                        int charges = (int)regenPoisonData["charges"];
                        int hpChange = (int)regenPoisonData["hpChange"];
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
                            this.world.colorOverlay.color = Color.Red * 0.8f;
                            this.world.colorOverlay.viewParams.Opacity = 0f;
                            this.world.colorOverlay.transManager.AddTransition(new Transition(transManager: this.world.colorOverlay.transManager, outTrans: true, duration: 12, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.5f));
                        }

                        // changing target hit points

                        int minValue = canKill ? 0 : 1;

                        this.boardPiece.hitPoints = Math.Min(this.boardPiece.hitPoints + hpChange, this.boardPiece.maxHitPoints);
                        this.boardPiece.hitPoints = Math.Max(this.boardPiece.hitPoints, minValue);
                        if (this.boardPiece.hitPoints == 0)
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

                default:
                    throw new DivideByZeroException($"Unsupported eventName - {eventName}.");
            }
        }

    }
}
