using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class HintEngine
    {
        public enum Type { Hungry, VeryHungry, Starving, Tired, VeryTired, CantShootInWater, CantUseToolInDeepWater, SmallInventory, MapNegative }

        private static readonly int hintDelay = 60 * 60; // 60 * 60
        private static readonly int blockInputDuration = 80;

        public List<Type> shownGeneralHints = new List<Type> { };
        public List<PieceHint.Name> shownPieceHints = new List<PieceHint.Name> { };
        public readonly World world;
        private int waitUntilFrame;

        public HintEngine(World world)
        {
            this.world = world;
            this.waitUntilFrame = 200;
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> hintsData = new Dictionary<string, object>
            {
                {"shownGeneralHints", this.shownGeneralHints },
                {"shownPieceHints", this.shownPieceHints },
                {"waitUntilFrame", this.waitUntilFrame },
            };

            return hintsData;
        }

        public void Deserialize(Dictionary<string, Object> hintsData)
        {
            this.shownGeneralHints = (List<Type>)hintsData["shownGeneralHints"];
            this.shownPieceHints = (List<PieceHint.Name>)hintsData["shownPieceHints"];
            this.waitUntilFrame = (int)hintsData["waitUntilFrame"];
        }

        public bool GeneralHintHasBeenShown(Type type)
        { return shownGeneralHints.Contains(type); }

        public bool PieceHintHasBeenShown(PieceHint.Name name)
        { return shownPieceHints.Contains(name); }

        public bool Show(Type type, bool ignoreDelay = false)
        {
            if (!Preferences.showHints) return false;

            if (!ignoreDelay)
            {
                if (this.world.player.activeState != BoardPiece.State.PlayerControlledWalking) return false;
                if (this.world.currentUpdate < this.waitUntilFrame) return false;
            }

            if (this.shownGeneralHints.Contains(type)) return false;

            this.waitUntilFrame = this.world.currentUpdate + hintDelay;

            switch (type)
            {
                case Type.Hungry:
                    {
                        var messageList = new List<string> { "I'm getting hungry.", "Time to look for something to eat.", "It would be a good idea to eat now.", "Hmm... Dinner time?" };
                        var message = messageList[this.world.random.Next(0, messageList.Count)];
                        this.DisableType(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.VeryHungry:
                    {
                        var messageList = new List<string> { "I'm really hungry.", "I'm getting really hungry." };
                        var message = messageList[this.world.random.Next(0, messageList.Count)];
                        this.DisableType(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.Starving:
                    {
                        var messageList = new List<string> { "I'm starving.\nI need to eat something right now or else I'm gonna die...", "I'm dying from hunger.", "I have to eat right now!" };
                        var message = messageList[this.world.random.Next(0, messageList.Count)];
                        this.DisableType(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.Tired:
                    {
                        var messageList = new List<string> { "I'm tired.", "I'm kinda sleepy.", "I'm exhausted." };
                        var message = messageList[this.world.random.Next(0, messageList.Count)];
                        this.DisableType(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.VeryTired:
                    {
                        var messageList = new List<string> { "I'm getting very sleepy.", "I'm so sleepy...", "I have to sleep now...", "I'm gonna collapse if I don't go to sleep now." };
                        var message = messageList[this.world.random.Next(0, messageList.Count)];
                        this.DisableType(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.CantShootInWater:
                    this.DisableType(type: type, delay: 60 * 60 * 10);
                    ShowMessageDuringPause("I cannot shoot while swimming.");

                    break;

                case Type.CantUseToolInDeepWater:
                    this.DisableType(type: type, delay: 60 * 60 * 10);
                    ShowMessageDuringPause("I cannot do this while swimming.");

                    break;

                case Type.SmallInventory:
                    {
                        this.DisableType(type: type, delay: 0);
                        ShowMessageDuringPause("I cannot carry many items right now.\nWith some leather I should be able to make a backpack.");
                        break;
                    }

                case Type.MapNegative:
                    {
                        this.DisableType(type: type, delay: 0);
                        ShowMessageDuringPause("I don't have a map.\nIf I had some leather and a workshop - I could make one.");
                        break;
                    }

                default:
                    { throw new DivideByZeroException($"Unsupported hint type - {type}."); }
            }

            return true;
        }

        public void CheckForPieceHintToShow(bool forcedMode = false, bool ignoreInputActive = false)
        {
            if (!Preferences.showHints || this.world.player.activeState != BoardPiece.State.PlayerControlledWalking) return;
            if (!forcedMode && this.world.currentUpdate < this.waitUntilFrame) return;

            bool hintShown = PieceHint.CheckForHintToShow(player: world.player, shownPieceHints: this.shownPieceHints, forcedMode: forcedMode, ignoreInputActive: ignoreInputActive);
            if (hintShown) this.waitUntilFrame = this.world.currentUpdate + hintDelay;
        }

        public static void ShowMessageDuringPause(string message)
        {
            new TextWindow(text: message, textColor: Color.Black, bgColor: Color.White, useTransition: true, animate: true, closingTask: Scheduler.TaskName.TempoPlay, blockInputDuration: blockInputDuration);
            new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null);
        }

        public static void ShowPieceDuringPause(World world, BoardPiece pieceToShow, string message)
        {
            world.camera.TrackPiece(pieceToShow);
            new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.CameraZoom, delay: 0, executeHelper: 2.5f);
            new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null);

            var closingTasks = new List<Object> { };
            closingTasks.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.CameraTrackPlayer, delay: 0, executeHelper: null, storeForLaterUse: true));
            closingTasks.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true));
            closingTasks.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.CameraZoom, delay: 0, executeHelper: 1f, storeForLaterUse: true));

            BoardPiece crossHair = PieceTemplate.CreateOnBoard(world: world, position: new Vector2(pieceToShow.sprite.gfxRect.Center.X, pieceToShow.sprite.gfxRect.Center.Y), templateName: PieceTemplate.Name.Crosshair);
            new Tracking(world: world, targetSprite: pieceToShow.sprite, followingSprite: crossHair.sprite);

            var worldEventData = new Dictionary<string, object> { { "boardPiece", crossHair }, { "delay", 60 }, { "eventName", WorldEvent.EventName.Destruction } };
            closingTasks.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.AddWorldEvent, delay: 0, executeHelper: worldEventData, storeForLaterUse: true));

            new TextWindow(text: message, textColor: Color.Black, bgColor: Color.White, useTransition: true, animate: true, closingTask: Scheduler.TaskName.ExecuteTaskList, closingTaskHelper: closingTasks, blockInputDuration: blockInputDuration);
        }
        public void RestoreAllHints()
        {
            this.shownPieceHints.Clear();
            this.shownGeneralHints.Clear();
            this.waitUntilFrame = 0;
        }

        private void DisableType(Type type, int delay = 0)
        {
            if (!this.shownGeneralHints.Contains(type)) this.shownGeneralHints.Add(type);

            if (delay != 0) new WorldEvent(eventName: WorldEvent.EventName.RestoreHint, delay: delay, world: this.world, boardPiece: null, eventHelper: type);
        }

        public void EnableType(Type type)
        {
            this.shownGeneralHints.Remove(type);
        }

    }
}
