using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class HintEngine
    {
        public enum Type { Empty, Hungry, VeryHungry, Starving, Tired, VeryTired, CantShootInWater, SmallInventory, MapNegative, Lava, BreakingItem, BrokenItem, BurntOutTorch, CineIntroduction, CineSmallBase, AnimalScaredOfFire, AnimalCounters };

        private static readonly List<Type> typesThatIgnoreShowHintSetting = new List<Type> { Type.CineIntroduction, Type.CineSmallBase, Type.VeryTired, Type.Starving, Type.BrokenItem, Type.BurntOutTorch };

        private static readonly int hintDelay = 1 * 60 * 60; // 1 * 60 * 60
        public static readonly int blockInputDuration = 80;

        public List<Type> shownGeneralHints = new List<Type> { };
        public List<PieceHint.Type> shownPieceHints = new List<PieceHint.Type> { };
        public List<Tutorials.Type> shownTutorials = new List<Tutorials.Type> { };
        public readonly World world;
        private int waitUntilFrame;
        public bool WaitFrameReached { get { return this.world.currentUpdate >= this.waitUntilFrame; } }

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
                {"shownTutorials", this.shownTutorials },
                {"waitUntilFrame", this.waitUntilFrame },
            };

            return hintsData;
        }

        public void Deserialize(Dictionary<string, Object> hintsData)
        {
            this.shownGeneralHints = (List<Type>)hintsData["shownGeneralHints"];
            this.shownPieceHints = (List<PieceHint.Type>)hintsData["shownPieceHints"];
            this.shownTutorials = (List<Tutorials.Type>)hintsData["shownTutorials"];
            this.waitUntilFrame = (int)hintsData["waitUntilFrame"];
        }

        public void UpdateWaitFrame()
        { this.waitUntilFrame = this.world.currentUpdate + hintDelay; }

        public bool ShowGeneralHint(Type type, bool ignoreDelay = false, string text = "", Texture2D texture = null, BoardPiece piece = null)
        {
            if ((!Preferences.showHints && !typesThatIgnoreShowHintSetting.Contains(type)) || Scene.GetTopSceneOfType(typeof(TextWindow)) != null) return false;

            if (!ignoreDelay)
            {
                if (this.world.player.activeState != BoardPiece.State.PlayerControlledWalking) return false;
                if (!this.WaitFrameReached) return false;
            }

            if (this.shownGeneralHints.Contains(type) || Scheduler.HasTaskChainInQueue) return false;

            this.UpdateWaitFrame();

            switch (type)
            {
                case Type.Empty:
                    {
                        // to allow for empty default action in PieceHint
                        break;
                    }

                case Type.Hungry:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm getting hungry.", blockInput: true),
                            new HintMessage(text: "Time to look for | | | something to eat.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.Tomato)}, blockInput: true),
                            new HintMessage(text: "It would be a good idea to | eat now.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInput: true),
                            new HintMessage(text: "Hmm... | Dinner time?", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal) }, blockInput: true),
                        };

                        this.world.player.soundPack.Play(PieceSoundPack.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(0, hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.VeryHungry:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm getting really | | | hungry.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.Tomato)}, blockInput: true),
                            new HintMessage(text: "I'm really | hungry.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInput: true),
                        };

                        this.world.player.soundPack.Play(PieceSoundPack.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(0, hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.Starving:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm starving.\nI need to eat | | | something right now\nor else I'm gonna | die...", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Apple), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.MeatRaw), AnimData.framesForPkgs[AnimData.PkgName.SkullAndBones].texture}, blockInput: true),
                            new HintMessage(text: "I'm | dying from | hunger.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.SkullAndBones].texture, PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInput: true),
                            new HintMessage(text: "| I have to | eat right now!", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Exclamation].texture, PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInput: true),
                        };

                        this.world.player.soundPack.Play(PieceSoundPack.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(0, hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.Tired:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm tired |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInput: true),
                            new HintMessage(text: "I'm kinda sleepy |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInput: true),
                            new HintMessage(text: "I'm exhausted |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInput: true),
                        };

                        this.world.player.soundPack.Play(PieceSoundPack.Action.PlayerYawn);

                        var message = hintMessages[this.world.random.Next(0, hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.VeryTired:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm getting very sleepy |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInput: true),
                            new HintMessage(text: "I'm so sleepy... |", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInput: true),
                            new HintMessage(text: "I have to sleep | now...", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInput: true),
                            new HintMessage(text: "I'm gonna collapse if I don't go to sleep | now.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInput: true),
                        };

                        this.world.player.soundPack.Play(PieceSoundPack.Action.PlayerYawn);

                        var message = hintMessages[this.world.random.Next(0, hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.CantShootInWater:
                    {
                        this.Disable(type: type, delay: 60 * 60 * 10);
                        ShowMessageDuringPause(new List<HintMessage> { new HintMessage(text: "I cannot | shoot while | swimming.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BowWood), AnimData.framesForPkgs[AnimData.PkgName.WaterDrop].texture }, blockInput: true) });
                        break;
                    }

                case Type.SmallInventory:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "I cannot carry many items right now.\nWith some | leather I should be able to make a | backpack.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall) }, blockInput: true) });
                        break;
                    }

                case Type.MapNegative:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: $"I don't have a map.\nIf I had some | leather and an | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopBasic).readableName} - I could make one.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopBasic)}, blockInput: true) });
                        break;
                    }

                case Type.Lava:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "Ouch! This is | lava!", imageList: new List<Texture2D> {AnimData.framesForPkgs[AnimData.PkgName.Flame].texture}, blockInput: true) });
                        break;
                    }

                case Type.BreakingItem:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: $"My | {text} is getting worn out.", blockInput: true, imageList: new List<Texture2D> { texture }),
                            new HintMessage(text: "I should watch my tools durability.", blockInput: true),
                        });
                        break;
                    }

                case Type.BrokenItem:
                    {
                        // no Disable(), because this hint should be shown every time
                        Sound.QuickPlay(SoundData.Name.DestroyWood);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: $"My | {text} has fell apart.", imageList: new List<Texture2D>{texture}, blockInput: true),
                        });
                        break;
                    }

                case Type.BurntOutTorch:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new HintMessage(text: $"My | {text} has burnt out.", imageList: new List<Texture2D> { texture }, blockInput: true));
                        break;
                    }

                case Type.AnimalScaredOfFire:
                    {
                        this.Disable(type: type, delay: 0);

                        ShowPieceDuringPause(world: world, pieceToShow: piece, messageList: new List<HintMessage> {
                            new HintMessage($"This | {piece.readableName} is scared of | fire!", imageList: new List<Texture2D> { piece.sprite.frame.texture, AnimData.framesForPkgs[AnimData.PkgName.Flame].texture}, blockInput: true),
                            new HintMessage("I think I'm safe | here.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Campfire)}, blockInput: true)
                        });
                        break;
                    }

                case Type.AnimalCounters:
                    {
                        this.Disable(type: type, delay: 0);

                        ShowPieceDuringPause(world: world, pieceToShow: piece, messageList: new List<HintMessage> {
                            new HintMessage($"This | {piece.readableName} had just attacked me!\nIt must be because I have | attacked it first...", imageList: new List<Texture2D> {piece.sprite.frame.texture, AnimData.framesForPkgs[AnimData.PkgName.BloodSplatter1].texture}, blockInput: true),
                        });
                        break;
                    }

                case Type.CineIntroduction:
                    {
                        this.Disable(type: type, delay: 0);

                        this.world.CineMode = true;
                        this.world.camera.SetZoom(zoom: 3f, setInstantly: true);

                        SolidColor whiteOverlay = new SolidColor(color: Color.White, viewOpacity: 1f);
                        this.world.solidColorManager.Add(whiteOverlay);

                        Player player = this.world.player;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        // HintMessage.ConvertToTasks() could be used here, but adding one by one makes it easier to add other task types between text.
                        var taskChain = new List<Object>();

                        taskChain.Add(new HintMessage(text: "Where am I?", boxType: dialogue, delay: 80, blockInput: false).ConvertToTask());
                        taskChain.Add(new HintMessage(text: "    ...    ", boxType: dialogue, delay: 120, blockInput: false).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 700 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "The last thing I remember...?", boxType: dialogue, delay: 60, blockInput: false).ConvertToTask());
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 30, executeHelper: new Dictionary<string, Object> { { "zoom", 1f }, { "zoomSpeedMultiplier", 0.1f } }, storeForLaterUse: true));
                        taskChain.Add(new HintMessage(text: "Hmm...\n...\n...", boxType: dialogue, delay: 60, blockInput: false).ConvertToTask());
                        taskChain.Add(new HintMessage(text: "There was... a terrible storm....", boxType: dialogue, delay: 90, blockInput: false).ConvertToTask());

                        Vector2 seaOffset = new Vector2(SonOfRobinGame.VirtualWidth * 0.65f, SonOfRobinGame.VirtualHeight * 0.65f);

                        int edgeDistLeft = (int)player.sprite.position.X;
                        int edgeDistRight = (int)(this.world.width - player.sprite.position.X);
                        int edgeDistUp = (int)player.sprite.position.Y;
                        int edgeDistDown = (int)(this.world.height - player.sprite.position.Y);
                        int edgeDistX = edgeDistLeft < edgeDistRight ? -edgeDistLeft : edgeDistRight;
                        int edgeDistY = edgeDistUp < edgeDistDown ? -edgeDistUp : edgeDistDown;
                        if (edgeDistX < 0) seaOffset.X *= -1;
                        if (edgeDistY < 0) seaOffset.Y *= -1;
                        if (Math.Abs(edgeDistX) < Math.Abs(edgeDistY)) seaOffset.Y = 0;
                        else seaOffset.X = 0;
                        Vector2 seaPos = player.sprite.position + seaOffset;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 170, executeHelper: new Dictionary<Player, Vector2> { { this.world.player, player.sprite.position + (seaOffset * 0.1f) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackCoords, delay: 40, executeHelper: seaPos, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "What happened to the ship?", boxType: dialogue, delay: 0).ConvertToTask());
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackPiece, delay: 60, executeHelper: world.player, storeForLaterUse: true));
                        taskChain.Add(new HintMessage(text: "I can't see it anywhere...", boxType: dialogue, delay: 0).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 40, executeHelper: new Dictionary<Player, Vector2> { { this.world.player, player.sprite.position } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 60, executeHelper: new Dictionary<string, Object> { { "zoom", 0.55f }, { "zoomSpeedMultiplier", 3f } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 0, executeHelper: SoundData.Name.DunDunDun, storeForLaterUse: true));
                        taskChain.Add(new HintMessage(text: "I guess I'm stranded | here.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.PalmTree].texture }, boxType: dialogue, delay: 0).ConvertToTask());
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CheckForPieceHints, delay: 10, executeHelper: new Dictionary<string, Object> { { "typesToCheckOnly", new List<PieceHint.Type> { PieceHint.Type.CrateStarting } } }, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                case Type.CineSmallBase:
                    {
                        this.Disable(type: type, delay: 0);

                        Player player = this.world.player;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.CineMode = true;

                        this.world.camera.SetZoom(zoom: 1.5f, setInstantly: false, zoomSpeedMultiplier: 0.4f);

                        var taskChain = new List<Object>();

                        taskChain.Add(new HintMessage(text: "Well...", boxType: dialogue, delay: 60).ConvertToTask());

                        Vector2 basePos = player.sprite.position;
                        int distance = 30;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 30, executeHelper: new Dictionary<Player, Vector2> { { this.world.player, basePos + new Vector2(-distance, 0) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 60, executeHelper: new Dictionary<Player, Vector2> { { this.world.player, basePos + new Vector2(distance, 0) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 30, executeHelper: new Dictionary<Player, Vector2> { { this.world.player, basePos + new Vector2(distance * 1.5f, distance) } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hmm...", boxType: dialogue, delay: 60).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.player, basePos } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 0.8f }, { "zoomSpeedMultiplier", 2f } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "This should be enough for basic camp | |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) }, boxType: dialogue, delay: 30).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                default:
                    { throw new DivideByZeroException($"Unsupported hint type - {type}."); }
            }

            return true;
        }

        public void CheckForPieceHintToShow(bool ignoreInputActive = false, List<PieceHint.Type> typesToCheckOnly = null, PieceTemplate.Name fieldPieceNameToCheck = PieceTemplate.Name.Empty, PieceTemplate.Name newOwnedPieceNameToCheck = PieceTemplate.Name.Empty)
        {
            if (!Preferences.showHints || this.world.player.activeState != BoardPiece.State.PlayerControlledWalking || Scene.GetTopSceneOfType(typeof(TextWindow)) != null) return;
            if (!this.WaitFrameReached && typesToCheckOnly == null && fieldPieceNameToCheck == PieceTemplate.Name.Empty && newOwnedPieceNameToCheck == PieceTemplate.Name.Empty) return;

            bool hintShown = PieceHint.CheckForHintToShow(hintEngine: this, player: world.player, ignoreInputActive: ignoreInputActive, typesToCheckOnly: typesToCheckOnly, fieldPieceNameToCheck: fieldPieceNameToCheck, newOwnedPieceNameToCheck: newOwnedPieceNameToCheck);
            if (hintShown) this.UpdateWaitFrame();
        }

        public static void ShowMessageDuringPause(HintMessage message)
        {
            ShowMessageDuringPause(new List<HintMessage> { message });
        }

        public static void ShowMessageDuringPause(List<HintMessage> messageList)
        {
            var taskChain = HintMessage.ConvertToTasks(messageList: messageList);

            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true));
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
        }

        public static void ShowPieceDuringPause(World world, BoardPiece pieceToShow, List<HintMessage> messageList)
        {
            world.camera.TrackPiece(pieceToShow);

            BoardPiece crossHair = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(pieceToShow.sprite.gfxRect.Center.X, pieceToShow.sprite.gfxRect.Center.Y), templateName: PieceTemplate.Name.Crosshair);
            new Tracking(world: world, targetSprite: pieceToShow.sprite, followingSprite: crossHair.sprite);

            var taskChain = HintMessage.ConvertToTasks(messageList: messageList);

            // tasks before the messages - inserted at 0, so the last go first

            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 2f } }, storeForLaterUse: true));

            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true));

            var worldEventData = new Dictionary<string, object> { { "boardPiece", crossHair }, { "delay", 60 }, { "eventName", WorldEvent.EventName.Destruction } };
            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.AddWorldEvent, delay: 0, executeHelper: worldEventData, storeForLaterUse: true));

            // task after the messages - added at the end, ordered normally

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackPiece, delay: 0, executeHelper: world.player, storeForLaterUse: true));
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 1f } }, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
        }
        public void RestoreAllHints()
        {
            this.shownPieceHints.Clear();
            this.shownGeneralHints.Clear();
            this.shownTutorials.Clear();
            this.waitUntilFrame = 0;
        }
        public void Enable(Type type)
        {
            this.shownGeneralHints.Remove(type);
        }

        private void Disable(Type type, int delay = 0)
        {
            if (!this.shownGeneralHints.Contains(type)) this.shownGeneralHints.Add(type);

            if (delay != 0) new WorldEvent(eventName: WorldEvent.EventName.RestoreHint, delay: delay, world: this.world, boardPiece: null, eventHelper: type);
        }

        public void Disable(PieceHint.Type type)
        { if (!this.shownPieceHints.Contains(type)) this.shownPieceHints.Add(type); }

        public void Disable(Tutorials.Type type)
        { if (!this.shownTutorials.Contains(type)) this.shownTutorials.Add(type); }


    }
}
