using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class HintEngine
    {
        public enum Type : byte
        {
            Empty = 0,
            Hungry = 1,
            VeryHungry = 2,
            Starving = 3,
            Tired = 4,
            VeryTired = 5,
            CantUseToolsInWater = 6,
            SmallInventory = 7,
            MapNegative = 8,
            Lava = 9,
            BreakingItem = 10,
            BrokenItem = 11,
            BurntOutTorch = 12,
            CineIntroduction = 13,
            CineSmallBase = 14,
            AnimalScaredOfFire = 15,
            AnimalCounters = 16,
            ZoomOutLocked = 17,
            Lightning = 18,
            TooDarkToUseTools = 19,
            BadSleep = 20,
        };

        private const int hintDelay = 1 * 60 * 60; // 1 * 60 * 60
        public const int blockInputDuration = 80;

        private static readonly List<Type> typesThatIgnoreShowHintSetting = new() { Type.CineIntroduction, Type.CineSmallBase, Type.VeryTired, Type.Starving, Type.BrokenItem, Type.BurntOutTorch, Type.Lava, Type.Lightning, Type.CantUseToolsInWater, Type.TooDarkToUseTools, Type.BadSleep };

        public HashSet<Type> shownGeneralHints = new() { };
        public HashSet<PieceHint.Type> shownPieceHints = new() { };
        public HashSet<Tutorials.Type> shownTutorials = new() { };
        public readonly World world;
        private int waitUntilFrame;

        public HintEngine(World world)
        {
            this.world = world;
            this.waitUntilFrame = 200;
        }

        public bool WaitFrameReached
        { get { return this.world.CurrentUpdate >= this.waitUntilFrame; } }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> hintsData = new Dictionary<string, object>
            {
                {"shownGeneralHints", this.shownGeneralHints },
                {"shownPieceHints", this.shownPieceHints },
                {"shownTutorials", this.shownTutorials },
            };

            return hintsData;
        }

        public void Deserialize(Dictionary<string, Object> hintsData)
        {
            this.shownGeneralHints = (HashSet<Type>)hintsData["shownGeneralHints"];
            this.shownPieceHints = (HashSet<PieceHint.Type>)hintsData["shownPieceHints"];
            this.shownTutorials = (HashSet<Tutorials.Type>)hintsData["shownTutorials"];
            this.waitUntilFrame = this.world.CurrentUpdate + 200;
        }

        public void UpdateWaitFrame()
        { this.waitUntilFrame = this.world.CurrentUpdate + hintDelay; }

        public bool ShowGeneralHint(Type type, bool ignoreDelay = false, string text = "", Texture2D texture = null, BoardPiece piece = null)
        {
            if ((!Preferences.showHints && !typesThatIgnoreShowHintSetting.Contains(type)) || Scene.GetTopSceneOfType(typeof(TextWindow)) != null) return false;

            if (!ignoreDelay)
            {
                if (this.world.Player.activeState != BoardPiece.State.PlayerControlledWalking) return false;
                if (!this.WaitFrameReached) return false;
            }

            if (this.shownGeneralHints.Contains(type) || Scheduler.HasTaskChainInQueue || this.world.Player.sleepMode != Player.SleepMode.Awake) return false;
            // only one hint should be shown at once - waitingScenes cause playing next scene after turning off CineMode (playing scene without game being paused)

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

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
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

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.Starving:
                    {
                        var hintMessages = new List<HintMessage> {
                            new HintMessage(text: "I'm starving.\nI need to eat | | | something right now\nor else I'm gonna | die...", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Apple), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.Meal), AnimData.croppedFramesForPkgs[AnimData.PkgName.SkullAndBones].texture}, blockInput: true),
                            new HintMessage(text: "I'm | dying from | hunger.", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.SkullAndBones].texture, PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInput: true),
                            new HintMessage(text: "| I have to | eat right now!", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.BubbleExclamationRed].texture, PieceInfo.GetTexture(PieceTemplate.Name.Meal)}, blockInput: true),
                        };

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerStomachGrowl);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
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

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerYawn);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
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

                        this.world.Player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerYawn);

                        var message = hintMessages[this.world.random.Next(hintMessages.Count)];
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(message);
                        break;
                    }

                case Type.CantUseToolsInWater:
                    {
                        // no Disable(), because this hint should be shown every time
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: $"I cannot use my | {text} in water.", blockInput: true, imageList: new List<Texture2D> { texture }),
                        });
                        break;
                    }

                case Type.TooDarkToUseTools:
                    {
                        // no Disable(), because this hint should be shown every time

                        MessageLog.Add(text: $"Too dark to use {text}.", bgColor: new Color(105, 3, 18), texture: texture, avoidDuplicates: true);
                        break;
                    }

                case Type.BadSleep:
                    {
                        this.Disable(type: type, delay: 0);

                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "Sleeping here was really uncomfortable.\nI should really build a | better place to sleep in...", blockInput: true, imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.TentMedium).texture }),
                        });
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
                            new HintMessage(text: $"I don't have a map.\nIf I had some | leather and a | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopLeatherBasic).readableName} - I could make one.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopLeatherBasic) }, blockInput: true) });
                        break;
                    }

                case Type.Lava:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "Ouch! This is | lava!", imageList: new List<Texture2D> {AnimData.croppedFramesForPkgs[AnimData.PkgName.Flame].texture}, blockInput: true) });
                        break;
                    }

                case Type.Lightning:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: "Aaah! Lightning just hit the water.", blockInput: true) });
                        break;
                    }

                case Type.BreakingItem:
                    {
                        this.Disable(type: type, delay: 0);
                        ShowMessageDuringPause(new List<HintMessage> {
                            new HintMessage(text: $"My | {text} is getting damaged. It won't last forever.", blockInput: true, imageList: new List<Texture2D> { texture }),
                            new HintMessage(text: "I should watch my tools durability.", blockInput: true),
                        });
                        break;
                    }

                case Type.BrokenItem:
                    {
                        // no Disable(), because this hint should be shown every time
                        this.world.Player.pieceInfo.Yield.DropDebris(piece: this.world.Player, debrisTypeListOverride: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisStone }, particlesToEmit: 7);

                        Sound.QuickPlay(SoundData.Name.DestroyWood);
                        ShowMessageDuringPause(new HintMessage(text: $"My | {text} has fallen apart.", imageList: new List<Texture2D> { texture }, blockInput: true));

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
                            new HintMessage($"This | {piece.readableName} is scared of | fire!", imageList: new List<Texture2D> { piece.sprite.CroppedAnimFrame.texture, AnimData.croppedFramesForPkgs[AnimData.PkgName.Flame].texture}, blockInput: true),
                            new HintMessage("I think that I'm safe | here.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CampfireSmall)}, blockInput: true)
                        });
                        break;
                    }

                case Type.AnimalCounters:
                    {
                        this.Disable(type: type, delay: 0);

                        ShowPieceDuringPause(world: world, pieceToShow: piece, messageList: new List<HintMessage> {
                            new HintMessage($"This | {piece.readableName} had just attacked me!\nIt must be because I have | attacked it first...", imageList: new List<Texture2D> {piece.sprite.CroppedAnimFrame.texture, AnimData.croppedFramesForPkgs[AnimData.PkgName.BloodSplatter1].texture}, blockInput: true),
                        });
                        break;
                    }

                case Type.ZoomOutLocked:
                    {
                        this.Disable(type: type, delay: 0);

                        Sound.QuickPlay(SoundData.Name.Error);

                        ShowMessageDuringPause(new HintMessage(text: "'Zoom out' is disabled, when world scale is less than 1.", boxType: HintMessage.BoxType.RedBox, blockInput: true));
                        break;
                    }

                case Type.CineIntroduction:
                    {
                        this.Disable(type: type, delay: 0);

                        Player player = this.world.Player;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.cineCurtains.showPercentage = 1f; // no transition here
                        this.world.CineMode = true;

                        // HintMessage.ConvertToTasks() could be used here, but adding one by one makes it easier to add other task types between text.
                        var taskChain = new List<Object>();

                        if (this.world.PlayerName == PieceTemplate.Name.PlayerTestDemoness)
                        {
                            var invertedDialogue = HintMessage.BoxType.InvertedDialogue;

                            this.world.camera.SetZoom(zoom: 1f, setInstantly: true);

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 30, executeHelper: new Dictionary<string, Object> { { "zoom", 2f }, { "zoomSpeedMultiplier", 0.5f } }, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "   ...    ", boxType: dialogue, delay: 0, blockInput: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 170, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, player.sprite.position + new Vector2(0, 10) } }, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorAddOverlay, delay: 0, executeHelper: new Dictionary<string, Object> { { "color", Color.Red }, { "opacity", 0.35f } }, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ActivateLightEngine, delay: 0, executeHelper: this.world.Player.sprite.lightEngine, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 0, executeHelper: SoundData.Name.FireBurst, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 3.5f }, { "zoomSpeedMultiplier", 3f } }, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "Hello there, human.", boxType: invertedDialogue, delay: 30, blockInput: false).ConvertToTask());
                            taskChain.Add(new HintMessage(text: "Don't you know how to read?", boxType: invertedDialogue, delay: 30, blockInput: false).ConvertToTask());
                            taskChain.Add(new HintMessage(text: "It was clearly stated, that i will BREAK your game.", boxType: invertedDialogue, delay: 80, blockInput: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 300 } }, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "And I WILL | RUIN your game experience.\nWith pleasure |.", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.SkullAndBones].texture, AnimData.croppedFramesForPkgs[AnimData.PkgName.Heart].texture }, boxType: invertedDialogue, delay: 0).ConvertToTask());

                            taskChain.Add(new HintMessage(text: "My contract says, that I'm here to help test this game, human.", boxType: invertedDialogue, delay: 0).ConvertToTask());

                            taskChain.Add(new HintMessage(text: "Is that clear? Yeah?\nThen let's get started |!", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.Flame].texture }, boxType: invertedDialogue, delay: 0).ConvertToTask());
                        }
                        else
                        {
                            new LevelEvent(eventName: LevelEvent.EventName.CheckForPieceHints, level: this.world.ActiveLevel, delay: 60 * 2, boardPiece: null, eventHelper: new Dictionary<string, Object> { { "typesToCheckOnly", new List<PieceHint.Type> { PieceHint.Type.CineCrateStarting } } }); // will be executed after playing this hint

                            this.world.camera.SetZoom(zoom: 3f, setInstantly: true);

                            SolidColor whiteOverlay = new(color: Color.White, viewOpacity: 1f);
                            this.world.solidColorManager.Add(whiteOverlay);

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetGlobalWorldEffect, delay: 0, executeHelper: new MosaicInstance(textureSize: new Vector2(this.world.CameraViewRenderTarget.Width, this.world.CameraViewRenderTarget.Height), blurSize: new Vector2(18, 18), framesLeft: -1), storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "Where am I?", boxType: dialogue, delay: 80, blockInput: false).ConvertToTask());

                            taskChain.Add(new HintMessage(text: "    ...    ", boxType: dialogue, delay: 120, blockInput: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 700 } }, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetGlobalWorldTweener, delay: 0, executeHelper: new Dictionary<string, Object> { { "startIntensity", 1f }, { "tweenIntensity", 0.5f }, { "autoreverse", false }, { "easing", "SineOut" }, { "durationSeconds", 5f } }, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "The last thing I remember...?", boxType: dialogue, delay: 60, blockInput: false).ConvertToTask());
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 30, executeHelper: new Dictionary<string, Object> { { "zoom", 1f }, { "zoomSpeedMultiplier", 0.1f } }, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "Hmm...\n...\n...", boxType: dialogue, delay: 60, blockInput: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetGlobalWorldTweener, delay: 0, executeHelper: new Dictionary<string, Object> { { "startIntensity", 0.5f }, { "tweenIntensity", 0f }, { "autoreverse", false }, { "easing", "SineOut" }, { "durationSeconds", 5f } }, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "There was... a terrible storm....", boxType: dialogue, delay: 90, blockInput: false).ConvertToTask());

                            Vector2 seaOffset = new(SonOfRobinGame.VirtualWidth * 0.65f, SonOfRobinGame.VirtualHeight * 0.65f);

                            int edgeDistLeft = (int)player.sprite.position.X;
                            int edgeDistRight = (int)(this.world.ActiveLevel.width - player.sprite.position.X);
                            int edgeDistUp = (int)player.sprite.position.Y;
                            int edgeDistDown = (int)(this.world.ActiveLevel.height - player.sprite.position.Y);
                            int edgeDistX = edgeDistLeft < edgeDistRight ? -edgeDistLeft : edgeDistRight;
                            int edgeDistY = edgeDistUp < edgeDistDown ? -edgeDistUp : edgeDistDown;
                            if (edgeDistX < 0) seaOffset.X *= -1;
                            if (edgeDistY < 0) seaOffset.Y *= -1;
                            if (Math.Abs(edgeDistX) < Math.Abs(edgeDistY)) seaOffset.Y = 0;
                            else seaOffset.X = 0;
                            Vector2 seaPos = player.sprite.position + seaOffset;

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 170, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, player.sprite.position + (seaOffset * 0.1f) } }, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetMovementSpeed, delay: 40, executeHelper: 0.15f, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackCoords, delay: 0, executeHelper: seaPos, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "What happened to the ship?", boxType: dialogue, delay: 0).ConvertToTask());
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackPiece, delay: 60, executeHelper: world.Player, storeForLaterUse: true));
                            taskChain.Add(new HintMessage(text: "I can't see it anywhere...", boxType: dialogue, delay: 0).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetGlobalWorldEffect, delay: 0, executeHelper: null, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetMovementSpeed, delay: 40, executeHelper: 1f, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, player.sprite.position } }, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 60, executeHelper: new Dictionary<string, Object> { { "zoom", 0.55f }, { "zoomSpeedMultiplier", 3f } }, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 0, executeHelper: SoundData.Name.DunDunDun, storeForLaterUse: true));
                            taskChain.Add(new HintMessage(text: "I guess I'm stranded | here.", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.PalmTree].texture }, boxType: dialogue, delay: 0).ConvertToTask());
                        }

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                case Type.CineSmallBase:
                    {
                        this.Disable(type: type, delay: 0);

                        Player player = this.world.Player;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.CineMode = true;

                        var taskChain = new List<Object>();

                        taskChain.Add(new HintMessage(text: "Well...", boxType: dialogue, delay: 60).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true)); // repeated, to make sure it will be executed (in case of waitingScenes being used)

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 1.5f }, { "zoomSpeedMultiplier", 0.4f } }, storeForLaterUse: true));

                        Vector2 basePos = player.sprite.position;
                        int distance = 30;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 30, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(-distance, 0) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 60, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(distance, 0) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 30, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(distance * 1.5f, distance) } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hmm...", boxType: dialogue, delay: 60).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 0.8f }, { "zoomSpeedMultiplier", 2f } }, storeForLaterUse: true));

                        if (this.world.PlayerName == PieceTemplate.Name.PlayerTestDemoness)
                        {
                            taskChain.Add(new HintMessage(text: "This camp | | looks like shit.\nDo lowly humans really live in such pitiful conditions?", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) }, boxType: dialogue, delay: 30).ConvertToTask());
                        }
                        else
                        {
                            taskChain.Add(new HintMessage(text: "This should be enough for a basic camp | |.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentMedium), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) }, boxType: dialogue, delay: 30).ConvertToTask());
                        }

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                default:
                    { throw new ArgumentException($"Unsupported hint type - {type}."); }
            }

            return true;
        }

        public void CheckForPieceHintToShow(bool ignorePlayerState = false, bool ignoreInputActive = false, List<PieceHint.Type> typesToCheckOnly = null, PieceTemplate.Name fieldPieceNameToCheck = PieceTemplate.Name.Empty, PieceTemplate.Name newOwnedPieceNameToCheck = PieceTemplate.Name.Empty)
        {
            if ((!ignorePlayerState && this.world.Player.activeState != BoardPiece.State.PlayerControlledWalking) || Scene.GetTopSceneOfType(typeof(TextWindow)) != null) return;
            if (!this.WaitFrameReached && typesToCheckOnly == null && fieldPieceNameToCheck == PieceTemplate.Name.Empty && newOwnedPieceNameToCheck == PieceTemplate.Name.Empty) return;

            bool hintShown = PieceHint.CheckForHintToShow(hintEngine: this, player: world.Player, ignoreInputActive: ignoreInputActive, typesToCheckOnly: typesToCheckOnly, fieldPieceNameToCheck: fieldPieceNameToCheck, newOwnedPieceNameToCheck: newOwnedPieceNameToCheck);
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

            BoardPiece crossHair = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: new Vector2(pieceToShow.sprite.GfxRect.Center.X, pieceToShow.sprite.GfxRect.Center.Y), templateName: PieceTemplate.Name.Crosshair);
            new Tracking(level: world.ActiveLevel, targetSprite: pieceToShow.sprite, followingSprite: crossHair.sprite);

            var taskChain = HintMessage.ConvertToTasks(messageList: messageList);

            // tasks before the messages - inserted at 0, so the last go first

            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true));

            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 2f } }, storeForLaterUse: true));

            var worldEventData = new Dictionary<string, object> { { "boardPiece", crossHair }, { "delay", 60 }, { "eventName", LevelEvent.EventName.Destruction } };
            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.AddWorldEvent, delay: 0, executeHelper: worldEventData, storeForLaterUse: true));

            // task after the messages - added at the end, ordered normally

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackPiece, delay: 0, executeHelper: world.Player, storeForLaterUse: true));
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

            MessageLog.Add(debugMessage: true, text: "Hints has been restored.");
        }

        public void Enable(Type type)
        {
            this.shownGeneralHints.Remove(type);
        }

        private void Disable(Type type, int delay = 0)
        {
            if (!this.shownGeneralHints.Contains(type)) this.shownGeneralHints.Add(type);
            if (delay != 0) new LevelEvent(eventName: LevelEvent.EventName.RestoreHint, delay: delay, level: this.world.IslandLevel, boardPiece: null, eventHelper: type);
        }

        public void Disable(PieceHint.Type type)
        { if (!this.shownPieceHints.Contains(type)) this.shownPieceHints.Add(type); }

        public void Disable(Tutorials.Type type)
        { if (!this.shownTutorials.Contains(type)) this.shownTutorials.Add(type); }
    }
}