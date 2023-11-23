using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
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
            SmallInventory = 6,
            MapNegative = 7,
            Lava = 8,
            BreakingItem = 9,
            BrokenItem = 10,
            BurntOutTorch = 11,
            CineIntroduction = 12,
            CineSmallBase = 13,
            CineEndingPart1 = 14,
            CineEndingPart2 = 15,
            AnimalScaredOfFire = 16,
            AnimalCounters = 17,
            ZoomOutLocked = 18,
            Lightning = 19,
            TooDarkToUseTools = 20,
            BadSleep = 21,
        };

        public static readonly Type[] allTypes = (Type[])Enum.GetValues(typeof(Type));

        private const int hintDelay = 1 * 60 * 60; // 1 * 60 * 60
        public const int blockInputDuration = 80;

        private static readonly List<Type> typesThatIgnoreShowHintSetting = new() { Type.CineIntroduction, Type.CineSmallBase, Type.CineEndingPart1, Type.CineEndingPart2, Type.VeryTired, Type.Starving, Type.BrokenItem, Type.BurntOutTorch, Type.Lava, Type.Lightning, Type.TooDarkToUseTools, Type.BadSleep };

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

                            Scheduler.ExecutionDelegate camZoomDlgt1 = () =>
                            { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 2f, zoomSpeedMultiplier: 0.5f); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 30, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "   ...    ", boxType: dialogue, delay: 0, blockInput: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 170, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, player.sprite.position + new Vector2(0, 10) } }, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorAddOverlay, delay: 0, executeHelper: new Dictionary<string, Object> { { "color", Color.Red }, { "opacity", 0.35f } }, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate activateLightEngineDlgt = () =>
                            { if (!this.world.HasBeenRemoved) this.world.Player.sprite.lightEngine.Activate(); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: activateLightEngineDlgt, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.PlaySoundByName, delay: 0, executeHelper: SoundData.Name.FireBurst, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                            { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 3.5f, zoomSpeedMultiplier: 3f); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt2, storeForLaterUse: true));

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

                            this.world.solidColorManager.Add(new(color: Color.White, viewOpacity: 1f));

                            Scheduler.ExecutionDelegate setGlobEffectDlgt = () =>
                            {
                                if (!world.HasBeenRemoved) world.globalEffect = new MosaicInstance(textureSize: new Vector2(this.world.FinalRenderTarget.Width, this.world.FinalRenderTarget.Height), blurSize: new Vector2(18, 18), framesLeft: -1);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: setGlobEffectDlgt, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "Where am I?", boxType: dialogue, delay: 80, blockInput: false).ConvertToTask());

                            taskChain.Add(new HintMessage(text: "    ...    ", boxType: dialogue, delay: 120, blockInput: false).ConvertToTask());

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 700 } }, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate setGlobalTweenerDlgt1 = () =>
                            {
                                if (this.world.HasBeenRemoved) return;
                                this.world.tweenerForGlobalEffect.CancelAndCompleteAll();
                                this.world.globalEffect.intensityForTweener = 1f;
                                this.world.tweenerForGlobalEffect.TweenTo(target: this.world.globalEffect, expression: effect => effect.intensityForTweener, toValue: 0.5f, duration: 5f).Easing(EasingFunctions.SineOut);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: setGlobalTweenerDlgt1, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "The last thing I remember...?", boxType: dialogue, delay: 60, blockInput: false).ConvertToTask());

                            Scheduler.ExecutionDelegate camZoomDlgt1 = () =>
                            {
                                if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.1f);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 30, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "Hmm...\n...\n...", boxType: dialogue, delay: 60, blockInput: false).ConvertToTask());

                            Scheduler.ExecutionDelegate setGlobalTweenerDlgt2 = () =>
                            {
                                if (this.world.HasBeenRemoved) return;
                                this.world.tweenerForGlobalEffect.CancelAndCompleteAll();
                                this.world.tweenerForGlobalEffect.TweenTo(target: this.world.globalEffect, expression: effect => effect.intensityForTweener, toValue: 0f, duration: 5f).Easing(EasingFunctions.SineOut);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: setGlobalTweenerDlgt2, storeForLaterUse: true));

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

                            Scheduler.ExecutionDelegate camMoveSpdDlgt1 = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetMovementSpeed(0.15f); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 40, executeHelper: camMoveSpdDlgt1, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate trackCoordsDlgt = () => { if (!this.world.HasBeenRemoved) this.world.camera.TrackCoords(position: seaPos, moveInstantly: false); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: trackCoordsDlgt, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "What happened to the ship?", boxType: dialogue, delay: 0).ConvertToTask());

                            Scheduler.ExecutionDelegate trackPieceDlgt = () => { if (!world.HasBeenRemoved) world.camera.TrackPiece(player); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60, executeHelper: trackPieceDlgt, storeForLaterUse: true));

                            taskChain.Add(new HintMessage(text: "I can't see it anywhere...", boxType: dialogue, delay: 0).ConvertToTask());

                            Scheduler.ExecutionDelegate clearGlobEffectDlgt = () =>
                            {
                                if (!world.HasBeenRemoved) world.globalEffect = null;
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: clearGlobEffectDlgt, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate camMoveSpdDlgt2 = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetMovementSpeed(1f); };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 40, executeHelper: camMoveSpdDlgt2, storeForLaterUse: true));

                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, player.sprite.position } }, storeForLaterUse: true));

                            Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                            {
                                if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.55f, zoomSpeedMultiplier: 3f);
                            };
                            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60, executeHelper: camZoomDlgt2, storeForLaterUse: true));

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

                        Scheduler.ExecutionDelegate camZoomDlgt1 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 1.5f, zoomSpeedMultiplier: 0.4f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                        Vector2 basePos = player.sprite.position;
                        int distance = 30;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 30, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(-distance, 0) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 60, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(distance, 0) } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 30, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos + new Vector2(distance * 1.5f, distance) } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hmm...", boxType: dialogue, delay: 60).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, basePos } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.8f, zoomSpeedMultiplier: 2f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt2, storeForLaterUse: true));

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

                case Type.CineEndingPart1:
                    {
                        // no disable code needed

                        BoardPiece boat = piece;
                        Player player = this.world.Player;
                        Random random = player.world.random;
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.CineMode = true;
                        this.world.cineCurtains.Enabled = true;

                        int fadeToWhiteDuration = 60 * 8;

                        SolidColor colorOverlay = new(color: Color.White, viewOpacity: 0.0f);
                        colorOverlay.transManager.AddTransition(new Transition(transManager: colorOverlay.transManager, outTrans: true, startDelay: 0, duration: fadeToWhiteDuration, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 1f, endRemoveScene: true));
                        this.world.solidColorManager.Add(colorOverlay);

                        Level openSeaLevel = new Level(type: Level.LevelType.OpenSea, hasWeather: true, plansWeather: false, hasWater: true, world: this.world, width: 100000, height: 8000, seed: 0);

                        var taskChain = new List<Object>();

                        Vector2 walkDistanceUnit = Vector2.Clamp(value1: player.sprite.position - boat.sprite.position, min: new Vector2(-80), max: new Vector2(80));
                        int rndDist = 100;

                        Vector2 walkPos1 = player.sprite.position + (walkDistanceUnit * 2) + new Vector2(random.Next(-rndDist, rndDist), random.Next(-rndDist, rndDist));
                        Vector2 walkPos2 = player.sprite.position + (walkDistanceUnit * 3) + new Vector2(random.Next(-rndDist, rndDist), random.Next(-rndDist, rndDist));
                        Vector2 walkPos3 = player.sprite.position + (walkDistanceUnit * 4) + new Vector2(random.Next(-rndDist, rndDist), random.Next(-rndDist, rndDist));
                        Vector2 walkPos4 = boat.sprite.position + ((walkPos3 - boat.sprite.position) / 3);

                        TimeSpan timeUntilDeparture = this.world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Morning) + TimeSpan.FromHours(2); // to depart at early morning

                        Scheduler.ExecutionDelegate clockAdvanceDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;
                            this.world.islandClock.Advance(amount: IslandClock.ConvertTimeSpanToUpdates(timeUntilDeparture), ignorePause: true, ignoreMultiplier: true);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: fadeToWhiteDuration / 4, executeHelper: clockAdvanceDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate clearWeatherDelegate = () =>
                        { if (!this.world.HasBeenRemoved) this.world.weather.RemoveAllEventsForDuration(TimeSpan.FromHours(24)); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: clearWeatherDelegate, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I can't believe it. I've managed to build this boat with my own two hands!", boxType: dialogue, delay: 60 * 1).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "The countless hours of work have finally paid off.", boxType: dialogue, delay: 60 * 1).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "At last, bidding farewell to this island!", boxType: dialogue, delay: 60 * 1).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, walkPos1 } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt1 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.4f, zoomSpeedMultiplier: 0.1f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Will I miss this place someday?", boxType: dialogue, delay: 60 * 1).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "The memories here will always hold a special place in my heart...", boxType: dialogue, delay: 60 * 1).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, walkPos2 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "It was my home for such a long time...\nSurvival amidst loneliness and relentless challenges.", boxType: dialogue, delay: 60 * 1).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, walkPos3 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Well... I can't stay here forever,\neven though I might miss this place one day.", boxType: dialogue, delay: 60 * 1).ConvertToTask());

                        Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.8f, zoomSpeedMultiplier: 0.15f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: camZoomDlgt2, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, walkPos4 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I hope that my | boat can take me back home safely...\nThe waves seem both menacing and inviting.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BoatCompleteStanding) }, boxType: dialogue, delay: 0).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetPlayerPointWalkTarget, delay: 0, executeHelper: new Dictionary<Player, Vector2> { { this.world.Player, boat.sprite.position } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt3 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.2f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: camZoomDlgt3, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Alright! It's time to head home!", boxType: dialogue, delay: 60 * 1).ConvertToTask());

                        int fadeToWhiteFrames = 60 * 2;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorAddOverlay, delay: 0, executeHelper: new Dictionary<string, Object> { { "color", Color.White }, { "opacity", 1f }, { "fadeInDurationFrames", fadeToWhiteFrames } }, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: fadeToWhiteFrames + 60, executeHelper: false, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate enterLevelDelegate = () => { if (!this.world.HasBeenRemoved) this.world.EnterNewLevel(openSeaLevel); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: enterLevelDelegate, storeForLaterUse: true));

                        new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);

                        break;
                    }

                case Type.CineEndingPart2:
                    {
                        // no disable code needed

                        Player player = this.world.Player;
                        player.sprite.orientation = Sprite.Orientation.right;
                        player.sprite.CharacterStand();
                        var dialogue = HintMessage.BoxType.Dialogue;

                        this.world.CineMode = true;
                        this.world.cineCurtains.showPercentage = 1f; // no transition here
                        this.world.weather.RemoveAllEventsForDuration(TimeSpan.FromDays(100)); // to make sure no weather events are present

                        this.world.solidColorManager.RemoveAll();

                        this.world.solidColorManager.Add(new(color: Color.White, viewOpacity: 1f));

                        var taskChain = new List<Object>();

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 60 * 10 } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate clockAdvanceDlgt1 = () =>
                        {
                            this.world.islandClock.Advance(amount: IslandClock.ConvertTimeSpanToUpdates(this.world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Noon)), ignorePause: true, ignoreMultiplier: true);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: clockAdvanceDlgt1, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate trackCoordsDlgt = () => { if (!world.HasBeenRemoved) this.world.camera.TrackCoords(position: player.sprite.position + new Vector2(SonOfRobinGame.VirtualWidth * 6, 0), moveInstantly: true); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 1, executeHelper: trackCoordsDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt1 = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 0.15f, setInstantly: true); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt1, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camMoveSpdDlgt = () => { if (!this.world.HasBeenRemoved) this.world.camera.SetMovementSpeed(0.08f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camMoveSpdDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate trackPieceDlgt = () => { if (!this.world.HasBeenRemoved) this.world.camera.TrackPiece(player); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 1, executeHelper: trackPieceDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate camZoomDlgt2 = () =>
                        { if (!this.world.HasBeenRemoved) this.world.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.025f); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 1, executeHelper: camZoomDlgt2, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I've been adrift for weeks now.\nThe days start to blend together...", boxType: dialogue, delay: 60 * 7).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "Since departing from the island, all I see is an endless sea.\nNo sight of land or ships.", boxType: dialogue, delay: 60 * 4).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "This heat is unbearable. I don't have any strength left to row...", boxType: dialogue, delay: 60 * 2).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true)); // weather won't get updated without this

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 0, executeHelper: new WeatherEvent(type: Weather.WeatherType.Clouds, intensity: 1f, startTime: DateTime.MinValue, duration: TimeSpan.FromHours(24), transitionLength: TimeSpan.FromMinutes(40)), storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 0, executeHelper: new WeatherEvent(type: Weather.WeatherType.Fog, intensity: 1f, startTime: DateTime.MinValue, duration: TimeSpan.FromHours(24), transitionLength: TimeSpan.FromMinutes(40)), storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Soon, I will run out of | | food...", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MeatDried), PieceInfo.GetTexture(PieceTemplate.Name.Apple) }, boxType: dialogue, delay: 60 * 2).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 0, executeHelper: new WeatherEvent(type: Weather.WeatherType.Wind, intensity: 1f, startTime: DateTime.MinValue, duration: TimeSpan.FromHours(24), transitionLength: TimeSpan.FromMinutes(10)), storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 0, executeHelper: new WeatherEvent(type: Weather.WeatherType.Rain, intensity: 1f, startTime: DateTime.MinValue, duration: TimeSpan.FromHours(24), transitionLength: TimeSpan.FromMinutes(15)), storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "I'm starting to miss my island already...\nMaybe it wasn't such a good idea to leave?", boxType: dialogue, delay: 60 * 4).ConvertToTask());

                        taskChain.Add(new HintMessage(text: "Finally, | rain! How refreshing!", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.WaterDrop].texture }, boxType: dialogue, delay: 60 * 4).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddWeatherEvent, delay: 60 * 5, executeHelper: new WeatherEvent(type: Weather.WeatherType.Lightning, intensity: 0.35f, startTime: DateTime.MinValue, duration: TimeSpan.FromSeconds(40), transitionLength: TimeSpan.FromSeconds(15)), storeForLaterUse: true));

                        Scheduler.ExecutionDelegate createBadWeatherDelegate = () => { if (!this.world.HasBeenRemoved) this.world.weather.CreateVeryBadWeatherForDuration(TimeSpan.FromHours(24)); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: createBadWeatherDelegate, storeForLaterUse: true));

                        int cameraWidth = this.world.camera.viewRect.Width;
                        int cameraHeight = this.world.camera.viewRect.Width;

                        VisualEffect orbiter = (VisualEffect)PieceTemplate.CreatePiece(world: this.world, templateName: PieceTemplate.Name.Orbiter);

                        Scheduler.ExecutionDelegate addOrbiterDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            orbiter.PlaceOnBoard(randomPlacement: false, position: player.sprite.position);
                            orbiter.universalFloat = 2f;
                            orbiter.visualAid = player;
                            this.world.camera.TrackPiece(orbiter);
                            this.world.camera.SetMovementSpeed(1f);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: addOrbiterDlgt, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Oh no, a storm approaches!", boxType: dialogue, delay: 0).ConvertToTask());

                        Scheduler.ExecutionDelegate updateOrbiterDlgt1 = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            VisualEffect orbiter = (VisualEffect)PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: player.sprite.position, templateName: PieceTemplate.Name.Orbiter, closestFreeSpot: true);
                            orbiter.universalFloat = 8f;
                            this.world.camera.SetMovementSpeed(2f);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 2, executeHelper: updateOrbiterDlgt1, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hold on tight! The boat's rocking violently!", boxType: dialogue, delay: 60 * 3).ConvertToTask());

                        Scheduler.ExecutionDelegate updateOrbiterDlgt2 = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            VisualEffect orbiter = (VisualEffect)PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: player.sprite.position, templateName: PieceTemplate.Name.Orbiter, closestFreeSpot: true);
                            orbiter.universalFloat = 16f;
                            this.world.camera.SetMovementSpeed(4f);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 60 * 2, executeHelper: updateOrbiterDlgt2, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Aaaaaaah!", boxType: dialogue, delay: 0).ConvertToTask());

                        int fadeInDuration = 60 * 3;

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorAddOverlay, delay: 0, executeHelper: new Dictionary<string, Object> { { "color", Color.Black }, { "opacity", 1f }, { "fadeInDurationFrames", fadeInDuration } }, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate clockAdvanceDlgt2 = () =>
                        {
                            if (this.world.HasBeenRemoved) return;
                            TimeSpan timeUntilMorning = this.world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Morning) + TimeSpan.FromHours(2);
                            this.world.islandClock.Advance(amount: IslandClock.ConvertTimeSpanToUpdates(timeUntilMorning), ignorePause: true, ignoreMultiplier: true);
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: fadeInDuration, executeHelper: clockAdvanceDlgt2, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate deleteOrbiterDlgt = () =>
                        {
                            if (this.world.HasBeenRemoved) return;

                            this.world.camera.TrackPiece(player);
                            this.world.camera.ResetMovementSpeed();
                            orbiter.visualAid = null; // has to be cleared, otherwise the player will be destroyed, too
                            orbiter.Destroy();
                        };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: deleteOrbiterDlgt, storeForLaterUse: true));

                        Scheduler.ExecutionDelegate clearWeatherDelegate = () =>
                        { if (!this.world.HasBeenRemoved) this.world.weather.RemoveAllEventsForDuration(TimeSpan.FromHours(48)); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: clearWeatherDelegate, storeForLaterUse: true));

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 60 * 4, executeHelper: new Dictionary<string, Object> { { "manager", this.world.solidColorManager }, { "delay", 60 * 5 } }, storeForLaterUse: true));

                        taskChain.Add(new HintMessage(text: "Hmm...", boxType: dialogue, delay: 60 * 5).ConvertToTask());
                        taskChain.Add(new HintMessage(text: "I must have passed out...", boxType: dialogue, delay: 60 * 2).ConvertToTask());
                        taskChain.Add(new HintMessage(text: "I'm glad that the storm is finally over.", boxType: dialogue, delay: 60 * 3).ConvertToTask());

                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 60 * 2, executeHelper: false, storeForLaterUse: true)); // for testing

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

            Scheduler.ExecutionDelegate camZoomDlgt1 = () => { if (!world.HasBeenRemoved) world.camera.SetZoom(zoom: 2f); };
            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt1, storeForLaterUse: true));

            Scheduler.ExecutionDelegate addLevelEventDlgt = () =>
            {
                if (world.HasBeenRemoved) return;
                new LevelEvent(eventName: LevelEvent.EventName.Destruction, level: world.ActiveLevel, delay: 60, boardPiece: crossHair);
            };
            taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: addLevelEventDlgt, storeForLaterUse: true));

            // task after the messages - added at the end, ordered normally

            Scheduler.ExecutionDelegate trackPieceDlgt = () => { if (!world.HasBeenRemoved) world.camera.TrackPiece(world.Player); };
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: trackPieceDlgt, storeForLaterUse: true));

            Scheduler.ExecutionDelegate camZoomDlgt2 = () => { if (!world.HasBeenRemoved) world.camera.SetZoom(zoom: 1f); };
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: camZoomDlgt2, storeForLaterUse: true));

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

        public void Disable(Type type, int delay = 0)
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